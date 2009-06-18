// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ExpressionResolverTest : ExpressionNodeTestBase
  {
    private ExpressionResolver _expressionResolver;
    private Expression<Func<int, bool>> _unresolvedLambda;

    public override void SetUp ()
    {
      base.SetUp();
      _unresolvedLambda = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _expressionResolver = new ExpressionResolver (SourceNode);
    }

    [Test]
    public void GetResolvedExpression ()
    {
      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));
      var result = _expressionResolver.GetResolvedExpression (_unresolvedLambda.Body, _unresolvedLambda.Parameters[0], ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedExpression_RemovesTransparentIdentifiers ()
    {
      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));
      var result = _expressionResolver.GetResolvedExpression (_unresolvedLambda.Body, _unresolvedLambda.Parameters[0], ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedExpression_DetectsSubQueries ()
    {
      var unresolvedExpressionWithSubQuery = 
          ExpressionHelper.CreateLambdaExpression<int, int> (i => (from x in ExpressionHelper.CreateQuerySource () select i).Count());
      var result = _expressionResolver.GetResolvedExpression (
          unresolvedExpressionWithSubQuery.Body, unresolvedExpressionWithSubQuery.Parameters[0], ClauseGenerationContext);

      Assert.That (result, Is.InstanceOfType (typeof (SubQueryExpression)));
    }

    [Test]
    public void GetResolvedExpression_DetectsSubQueries_RegistersSubQueriesInList ()
    {
      var unresolvedExpressionWithSubQuery =
          ExpressionHelper.CreateLambdaExpression<int, int> (i => (from x in ExpressionHelper.CreateQuerySource () select i).Count ());
      var result = _expressionResolver.GetResolvedExpression (
          unresolvedExpressionWithSubQuery.Body, unresolvedExpressionWithSubQuery.Parameters[0], ClauseGenerationContext);

      Assert.That (ClauseGenerationContext.SubQueryRegistry.Contains (((SubQueryExpression) result).QueryModel));
    }

    [Test]
    public void GetResolvedExpression_DetectsSubQueries_AfterResolving ()
    {
      var unresolvedExpressionWithSubQuery =
          ExpressionHelper.CreateLambdaExpression<int, int> (i => (from x in ExpressionHelper.CreateQuerySource () select i).Count ());
      var result = _expressionResolver.GetResolvedExpression (
          unresolvedExpressionWithSubQuery.Body, unresolvedExpressionWithSubQuery.Parameters[0], ClauseGenerationContext);

      var subQueryExpression = (SubQueryExpression) result;
      var subQuerySelector = ((SelectClause) subQueryExpression.QueryModel.SelectOrGroupClause).Selector;
      Assert.That (subQuerySelector, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) subQuerySelector).ReferencedClause, Is.SameAs (SourceClause));
    }

    [Test]
    public void GetResolvedExpression_UsesNodeTypeRegistry ()
    {
      var nodeTypeRegistry = new MethodCallExpressionNodeTypeRegistry ();
      var context = new ClauseGenerationContext(QuerySourceClauseMapping, nodeTypeRegistry, new SubQueryRegistry());

      var unresolvedExpressionWithSubQuery =
          ExpressionHelper.CreateLambdaExpression<int, int> (i => (from x in ExpressionHelper.CreateQuerySource () select i).Count ());
      var result = _expressionResolver.GetResolvedExpression (
          unresolvedExpressionWithSubQuery.Body, unresolvedExpressionWithSubQuery.Parameters[0], context);

      Assert.That (result, Is.InstanceOfType (typeof (MethodCallExpression)), 
          "The given nodeTypeRegistry does not know any query methods, so no SubQueryExpression is generated.");
    }
  }
}