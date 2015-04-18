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
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors
{
  [TestFixture]
  public class ReplacingExpressionVisitorTest
  {
    private ParameterExpression _replacedNode;
    private ParameterExpression _replacementNode;

    [SetUp]
    public void SetUp ()
    {
      _replacedNode = ExpressionHelper.CreateParameterExpression ("replaced node");
      _replacementNode = ExpressionHelper.CreateParameterExpression ("replacement node");
    }

    [Test]
    public void ReplacesGivenNode_ByGivenReplacement ()
    {
      var tree = _replacedNode;

      var result = ReplacingExpressionVisitor.Replace (_replacedNode, _replacementNode, tree);
      Assert.That (result, Is.SameAs (_replacementNode));
    }

    [Test]
    public void IgnoresTree_WhenReplacedNodeDoesNotExist ()
    {
      var tree = ExpressionHelper.CreateLambdaExpression();

      var result = ReplacingExpressionVisitor.Replace (_replacedNode, _replacementNode, tree);
      Assert.That (result, Is.SameAs (tree));
    }

    [Test]
    public void ReplacesTreePart ()
    {
      var tree = Expression.MakeBinary (ExpressionType.Add, Expression.Constant (0), _replacedNode);

      var result = ReplacingExpressionVisitor.Replace (_replacedNode, _replacementNode, tree);

      var expectedResult = Expression.MakeBinary (ExpressionType.Add, Expression.Constant (0), _replacementNode);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ReplacesTreePart_InSubQueries ()
    {
      var replacedNode = ExpressionHelper.CreateQueryable<Cook> ().Expression;
      var replacementNode = Expression.Constant (null, typeof (Cook[]));

      var subQueryMainFromClause = new MainFromClause ("c", typeof (Cook), replacedNode);
      var subQuery = ExpressionHelper.CreateQueryModel (subQueryMainFromClause);

      var tree = new SubQueryExpression (subQuery);

      ReplacingExpressionVisitor.Replace (replacedNode, replacementNode, tree);

      Assert.That (subQueryMainFromClause.FromExpression, Is.SameAs (replacementNode));
    }

    [Test]
    public void VisitUnknownNonExtensionExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = ReplacingExpressionVisitor.Replace (_replacedNode, _replacementNode, expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExtensionExpression_DescendsIntoChildren ()
    {
      var tree = new VBStringComparisonExpression (_replacedNode, true);

      var result = ReplacingExpressionVisitor.Replace (_replacedNode, _replacementNode, tree);

      var expected = new VBStringComparisonExpression (_replacementNode, true);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }
  }
}
