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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class TupleNewExpressionTransformerTest
  {
    private TupleNewExpressionTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _transformer = new TupleNewExpressionTransformer ();
    }

    [Test]
    public void SupportedExpressionTypes ()
    {
      Assert.That (_transformer.SupportedExpressionTypes, Is.EqualTo (new[] { ExpressionType.New }));
    }

    [Test]
    public void Transform_OtherExpression ()
    {
      var expression = Expression.New (typeof (List<int>).GetConstructor (new[] { typeof (int) }), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_Tuple2_ValueCtor ()
    {
      var expression = Expression.New (
          typeof (Tuple<string, int>).GetConstructor (new[] { typeof (string), typeof (int) }),
          Expression.Constant ("test"),
          Expression.Constant (0));

      var result = _transformer.Transform (expression);

      if (Environment.Version.Major == 2)
      {
        var expectedExpression35 = Expression.New (
            typeof (Tuple<string, int>).GetConstructor (new[] { typeof (string), typeof (int) }),
            new Expression[] { Expression.Constant ("test"), Expression.Constant (0) },
            new MemberInfo[] { typeof (Tuple<string, int>).GetMethod ("get_Item1"), typeof (Tuple<string, int>).GetMethod ("get_Item2") });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression35, result);
      }
      else
      {
        var expectedExpression40 = Expression.New (
            typeof (Tuple<string, int>).GetConstructor (new[] { typeof (string), typeof (int) }),
            new Expression[] { Expression.Constant ("test"), Expression.Constant (0) },
            new MemberInfo[] { typeof (Tuple<string, int>).GetProperty ("Item1"), typeof (Tuple<string, int>).GetProperty ("Item2") });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression40, result);
      }
    }

    [Test]
    public void Transform_Tuple3_ValueCtor ()
    {
      var expression = Expression.New (
          typeof (Tuple<string, int, bool>).GetConstructor (new[] { typeof (string), typeof (int), typeof (bool) }),
          Expression.Constant ("test"),
          Expression.Constant (0),
          Expression.Constant (true));

      var result = _transformer.Transform (expression);

      if (Environment.Version.Major == 2)
      {
        var expectedExpression35 = Expression.New (
            typeof (Tuple<string, int, bool>).GetConstructor (new[] { typeof (string), typeof (int), typeof (bool) }),
            new Expression[] { Expression.Constant ("test"), Expression.Constant (0), Expression.Constant (true) },
            new MemberInfo[]
            {
                typeof (Tuple<string, int, bool>).GetMethod ("get_Item1"),
                typeof (Tuple<string, int, bool>).GetMethod ("get_Item2"),
                typeof (Tuple<string, int, bool>).GetMethod ("get_Item3")
            });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression35, result);
      }
      else
      {
        var expectedExpression40 = Expression.New (
            typeof (Tuple<string, int, bool>).GetConstructor (new[] { typeof (string), typeof (int), typeof (bool) }),
            new Expression[] { Expression.Constant ("test"), Expression.Constant (0), Expression.Constant (true) },
            new MemberInfo[]
            {
                typeof (Tuple<string, int, bool>).GetProperty ("Item1"),
                typeof (Tuple<string, int, bool>).GetProperty ("Item2"),
                typeof (Tuple<string, int, bool>).GetProperty ("Item3")
            });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression40, result);
      }
    }

    [Test]
    public void Transform_Tuple_ValueCtor_WithMembers_IsIgnored ()
    {
      var expression = Expression.New (
          typeof (Tuple<string, int, bool>).GetConstructor (new[] { typeof (string), typeof (int), typeof (bool) }),
          new Expression[] { Expression.Constant ("test"), Expression.Constant (0), Expression.Constant (true) },
          new MemberInfo[]
          {
              typeof (Tuple<string, int, bool>).GetProperty ("Item1"), 
              typeof (Tuple<string, int, bool>).GetProperty ("Item2"),
              typeof (Tuple<string, int, bool>).GetProperty ("Item3")
          });

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }
  }
}