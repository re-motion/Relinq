// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitorTests
{
  [TestFixture]
  public class RelinqExpressionVisitorTest : RelinqExpressionVisitorTestBase
  {
    [Test]
    public void AdjustArgumentsForNewExpression ()
    {
      var arguments = new[] { Expression.Constant (0), Expression.Constant ("string1"), Expression.Constant ("string2") };
      var tupleType = typeof (Tuple<double, object, string>);
      var members = new MemberInfo[] { tupleType.GetProperty ("Item1"), tupleType.GetMethod ("get_Item2"), tupleType.GetProperty ("Item3") };

      var result = RelinqExpressionVisitor.AdjustArgumentsForNewExpression (arguments, members).ToArray();

      Assert.That (result.Length, Is.EqualTo (3));
      var expected1 = Expression.Convert (arguments[0], typeof (double));
      ExpressionTreeComparer.CheckAreEqualTrees (expected1, result[0]);
      var expected2 = Expression.Convert (arguments[1], typeof (object));
      ExpressionTreeComparer.CheckAreEqualTrees (expected2, result[1]);
      Assert.That (result[2], Is.SameAs (arguments[2]));
    }

    [Test]
    public void AdjustArgumentsForNewExpression_WithMemberCountMismatch_ThrowsArgumentException ()
    {
      var arguments = new[] { Expression.Constant (0), Expression.Constant ("string1") };
      var tupleType = typeof (Tuple<double, object>);
      var members = new MemberInfo[] { tupleType.GetProperty ("Item1") };

      Assert.That (
          () => RelinqExpressionVisitor.AdjustArgumentsForNewExpression (arguments, members).ToArray(),
          Throws.ArgumentException.With.Message.EqualTo ("Incorrect number of arguments for the given members."));
    }
  }
}