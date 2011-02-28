// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel.TestDomain;
using Remotion.Linq.UnitTests.Linq.Core.TestUtilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
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
      Assert.That (subQuerySelector, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) subQuerySelector).ReferencedQuerySource, Is.SameAs (SourceClause));
    }
  }
}
