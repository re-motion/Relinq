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
using Remotion.Development.UnitTesting;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ExpressionResolverTest : ExpressionNodeTestBase
  {
    private ExpressionResolver _expressionResolver;
    private Expression<Func<int, bool>> _unresolvedLambda;
    private IExpressionNode _currentNode;

    public override void SetUp ()
    {
      base.SetUp();
      _unresolvedLambda = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _currentNode = new TestMethodCallExpressionNode (CreateParseInfo (), null);
      _expressionResolver = new ExpressionResolver (_currentNode);
    }

    [Test]
    public void GetResolvedExpression ()
    {
      var result = _expressionResolver.GetResolvedExpression (_unresolvedLambda.Body, _unresolvedLambda.Parameters[0], ClauseGenerationContext);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedExpression_NoticesChangesOfSourceNode ()
    {
      var newSourceNode = ExpressionNodeObjectMother.CreateMainSource ();
      var newQueryModel = newSourceNode.Apply (null, ClauseGenerationContext);
      var newSourceReference = ((QuerySourceReferenceExpression) newQueryModel.SelectClause.Selector);

      PrivateInvoke.InvokeNonPublicMethod (_currentNode, "set_Source", newSourceNode);
      var result = _expressionResolver.GetResolvedExpression (_unresolvedLambda.Body, _unresolvedLambda.Parameters[0], ClauseGenerationContext);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, newSourceReference, Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedExpression_RemovesTransparentIdentifiers ()
    {
      var result = _expressionResolver.GetResolvedExpression (_unresolvedLambda.Body, _unresolvedLambda.Parameters[0], ClauseGenerationContext);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedExpression_WithSubQueries ()
    {
      var parameterExpression = Expression.Parameter (typeof (int), "i");
      var subQueryExpression = new SubQueryExpression (ExpressionHelper.CreateQueryModel_Int ());
      subQueryExpression.QueryModel.SelectClause.Selector = parameterExpression;

      var result = (SubQueryExpression) _expressionResolver.GetResolvedExpression (subQueryExpression, parameterExpression, ClauseGenerationContext);

      var subQuerySelector = result.QueryModel.SelectClause.Selector;
      Assert.That (subQuerySelector, Is.InstanceOf (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) subQuerySelector).ReferencedQuerySource, Is.SameAs (SourceClause));
    }
  }
}
