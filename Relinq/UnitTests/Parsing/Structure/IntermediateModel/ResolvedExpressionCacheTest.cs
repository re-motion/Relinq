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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel.TestDomain;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ResolvedExpressionCacheTest
  {
    private ResolvedExpressionCache<Expression> _cache;

    [SetUp]
    public void SetUp ()
    {
      var parseInfo = new MethodCallExpressionParseInfo("x", ExpressionNodeObjectMother.CreateMainSource(), ExpressionHelper.CreateMethodCallExpression<Cook>());
      _cache = new ResolvedExpressionCache<Expression> (new TestMethodCallExpressionNode (parseInfo, null));
    }

    [Test]
    public void GetOrCreate_Initial ()
    {
      var fakeResult = ExpressionHelper.CreateExpression ();
      var result = _cache.GetOrCreate (r => fakeResult);

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetOrCreate_Twice ()
    {
      var fakeResult = ExpressionHelper.CreateExpression ();
      var result1 = _cache.GetOrCreate (r => fakeResult);

      var calledASecondTime = false;
      var result2 = _cache.GetOrCreate (r => { calledASecondTime = true; return null; } );

      Assert.That (result2, Is.SameAs (result1));
      Assert.That (calledASecondTime, Is.False);
    }
  }
}
