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
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors
{
  [TestFixture]
  public class MultiReplacingExpressionTreeVisitorTest
  {
    private ParameterExpression _replacedNode1;
    private ParameterExpression _replacementNode1;
    private ParameterExpression _replacedNode2;
    private ParameterExpression _replacementNode2;

    private Dictionary<Expression, Expression> _mapping;

    [SetUp]
    public void SetUp ()
    {
      _replacedNode1 = ExpressionHelper.CreateParameterExpression ("replaced node 1");
      _replacementNode1 = ExpressionHelper.CreateParameterExpression ("replacement node 1");

      _replacedNode2 = ExpressionHelper.CreateParameterExpression ("replaced node 2");
      _replacementNode2 = ExpressionHelper.CreateParameterExpression ("replacement node 2");

      _mapping = new Dictionary<Expression, Expression> { { _replacedNode1, _replacementNode1 }, { _replacedNode2, _replacementNode2 } };
    }

    [Test]
    public void ReplacesGivenNode_ByReplacement ()
    {
      var tree = _replacedNode2;

      var result = MultiReplacingExpressionTreeVisitor.Replace (_mapping, tree);
      Assert.That (result, Is.SameAs (_replacementNode2));
    }

    [Test]
    public void IgnoresTree_WhenReplacedNodesDoNotExist ()
    {
      var tree = ExpressionHelper.CreateLambdaExpression();

      var result = MultiReplacingExpressionTreeVisitor.Replace (_mapping, tree);
      Assert.That (result, Is.SameAs (tree));
    }

    [Test]
    public void ReplacesTreeParts ()
    {
      var tree = Expression.MakeBinary (ExpressionType.Add, _replacedNode1, _replacedNode2);

      var result = MultiReplacingExpressionTreeVisitor.Replace (_mapping, tree);

      var expectedResult = Expression.MakeBinary (ExpressionType.Add, _replacementNode1, _replacementNode2);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ReplacesTreePart_InSubQueries ()
    {
      var replacedNode = ExpressionHelper.CreateQueryable<Cook> ().Expression;
      var replacementNode = Expression.Constant (null, typeof (Cook[]));
      var mapping = new Dictionary<Expression, Expression> { { replacedNode, replacementNode } };

      var subQueryMainFromClause = new MainFromClause ("c", typeof (Cook), replacedNode);
      var subQuery = ExpressionHelper.CreateQueryModel (subQueryMainFromClause);

      var tree = new SubQueryExpression (subQuery);

      MultiReplacingExpressionTreeVisitor.Replace (mapping, tree);

      Assert.That (subQueryMainFromClause.FromExpression, Is.SameAs (replacementNode));
    }

    [Test]
    public void VisitUnknownNonExtensionExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = MultiReplacingExpressionTreeVisitor.Replace (_mapping, expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExtensionExpression_DescendsIntoChildren ()
    {
      var tree = new VBStringComparisonExpression (_replacedNode1, true);

      var result = MultiReplacingExpressionTreeVisitor.Replace (_mapping, tree);

      var expected = new VBStringComparisonExpression (_replacementNode1, true);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }
  }
}
