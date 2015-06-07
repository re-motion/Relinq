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
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.Expressions
{
  [TestFixture]
  public class SubQueryExpressionTest : ExpressionNodeTestBase
  {
    [Test]
    public void NodeType ()
    {
      Assert.That (SubQueryExpression.ExpressionType, Is.EqualTo ((ExpressionType) 100002));
      ExtensionExpressionTestHelper.CheckUniqueNodeType (typeof (SubQueryExpression), SubQueryExpression.ExpressionType);
    }

    [Test]
    public void Initialization ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook>();
      var expression = new SubQueryExpression (queryModel);
      Assert.That (expression.Type, Is.SameAs (typeof(IQueryable<Cook>)));
      Assert.That (expression.QueryModel, Is.SameAs (queryModel));
      Assert.That (expression.NodeType, Is.EqualTo (SubQueryExpression.ExpressionType));
    }


#if !NET_3_5
    [Test]
    public void TestToString ()
    {
      var queryExpression = ExpressionHelper.MakeExpression (() => (from s in ExpressionHelper.CreateQueryable<Cook> () select s).Count());
      var subQueryModel = ExpressionHelper.ParseQuery (queryExpression);
      var expression = Expression.MakeBinary (ExpressionType.GreaterThan, new SubQueryExpression (subQueryModel), Expression.Constant (2));

      var formattedExpression = expression.ToString();

      Assert.That (formattedExpression, Is.EqualTo ("({PartiallyEvaluated(CreateQueryable(), TestQueryable<Cook>()) => Count()} > 2)"));
    }
#endif
  }
}