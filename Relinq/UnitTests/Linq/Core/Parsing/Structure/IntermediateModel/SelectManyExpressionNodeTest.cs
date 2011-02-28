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
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class SelectManyExpressionNodeTest : ExpressionNodeTestBase
  {
    private Expression<Func<int, double[]>> _collectionSelector;
    private Expression<Func<int, int, bool>> _resultSelector;
    private SelectManyExpressionNode _nodeWithResultSelector;

    public override void SetUp ()
    {
      base.SetUp();

      _collectionSelector = ExpressionHelper.CreateLambdaExpression<int, double[]> (i => new double[1]);
      _resultSelector = ExpressionHelper.CreateLambdaExpression<int, int, bool> ((i, j) => i > j);
      _nodeWithResultSelector = new SelectManyExpressionNode (CreateParseInfo (SourceNode, "j"), _collectionSelector, _resultSelector);
    }

    [Test]
    public void SupportedMethod_WithoutPosition ()
    {
      AssertSupportedMethod_Generic (SelectManyExpressionNode.SupportedMethods, q => q.SelectMany (i => new[] { 1, 2, 3 }, (i, j) => new { i, j }), e => e.SelectMany (i => new[] { 1, 2, 3 }, (i, j) => new { i, j }));
    }

    [Test]
    public void SupportedMethod_WithoutResultOperator ()
    {
      AssertSupportedMethod_Generic (SelectManyExpressionNode.SupportedMethods, q => q.SelectMany (i => new[] { 1, 2, 3 }), e => e.SelectMany (i => new[] { 1, 2, 3 }));
    }

    [Test]
    public void Initialization_WithResultSelector ()
    {
      Assert.That (_nodeWithResultSelector.ResultSelector, Is.SameAs (_resultSelector));
    }

    [Test]
    public void Initialization_WithoutResultSelector ()
    {
      var nodeWithoutWithResultSelector = new SelectManyExpressionNode (CreateParseInfo (SourceNode, "j"), _collectionSelector, null);
      var expectedResultSelectorParameter1 = Expression.Parameter (typeof (int), "i");
      var expectedResultSelectorParameter2 = Expression.Parameter (typeof (double), "j");
      var expectedResultSelector = Expression.Lambda (expectedResultSelectorParameter2, expectedResultSelectorParameter1, expectedResultSelectorParameter2);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResultSelector, nodeWithoutWithResultSelector.ResultSelector);
    }

    [Test]
    public void Resolve_ReplacesParameter_WithProjection ()
    {
      var node = new SelectManyExpressionNode (
          CreateParseInfo(),
          _collectionSelector,
          ExpressionHelper.CreateLambdaExpression<int, int, AnonymousType> ((a, b) => new AnonymousType (a, b)));
      node.Apply (QueryModel, ClauseGenerationContext);
      var clause = (FromClauseBase) QueryModel.BodyClauses[0];

      var expression = ExpressionHelper.CreateLambdaExpression<AnonymousType, bool> (i => i.a > 5 && i.b > 6);
      var result = node.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext);

      var selectManySourceReference = new QuerySourceReferenceExpression (clause);

      // new AnonymousType (SourceReference, selectManySourceReference).a > 5 && new AnonymousType (SourceReference, selectManySourceReference).b > 6

      var newAnonymousTypeExpression = Expression.New (
          typeof (AnonymousType).GetConstructor (new[] { typeof (int), typeof (int) }), SourceReference, selectManySourceReference);
      var anonymousTypeMemberAExpression = Expression.MakeMemberAccess (newAnonymousTypeExpression, typeof (AnonymousType).GetProperty ("a"));
      var anonymousTypeMemberBExpression = Expression.MakeMemberAccess (newAnonymousTypeExpression, typeof (AnonymousType).GetProperty ("b"));

      var expectedResult = Expression.MakeBinary (
          ExpressionType.AndAlso,
          Expression.MakeBinary (ExpressionType.GreaterThan, anonymousTypeMemberAExpression, Expression.Constant (5)),
          Expression.MakeBinary (ExpressionType.GreaterThan, anonymousTypeMemberBExpression, Expression.Constant (6)));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedResultSelector ()
    {
      _nodeWithResultSelector.Apply (QueryModel, ClauseGenerationContext);
      var clause = (FromClauseBase) QueryModel.BodyClauses[0];
      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, new QuerySourceReferenceExpression (clause));

      var result = _nodeWithResultSelector.GetResolvedResultSelector (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve an IQuerySource for the given SelectManyExpressionNode. "
        + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both.")]
    public void GetResolvedResultSelector_WithoutClause ()
    {
      _nodeWithResultSelector.GetResolvedResultSelector (ClauseGenerationContext);
    }

    [Test]
    public void GetResolvedCollectionSelector ()
    {
      var expectedResult = Expression.NewArrayBounds (typeof (double), Expression.Constant (1));

      var result = _nodeWithResultSelector.GetResolvedCollectionSelector (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _nodeWithResultSelector.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var clause = (AdditionalFromClause) QueryModel.BodyClauses[0];

      Assert.That (clause.ItemName, Is.EqualTo ("j"));
      Assert.That (clause.ItemType, Is.SameAs (typeof (int)));
      Assert.That (clause.FromExpression, Is.SameAs (_nodeWithResultSelector.GetResolvedCollectionSelector (ClauseGenerationContext)));
    }

    [Test]
    public void Apply_AddsMapping ()
    {
      _nodeWithResultSelector.Apply (QueryModel, ClauseGenerationContext);
      var clause = (AdditionalFromClause) QueryModel.BodyClauses[0];

      Assert.That (ClauseGenerationContext.GetContextInfo (_nodeWithResultSelector), Is.SameAs (clause));
    }

    [Test]
    public void Apply_AdaptsSelector ()
    {
      _nodeWithResultSelector.Apply (QueryModel, ClauseGenerationContext);
      var clause = QueryModel.SelectClause;

      Assert.That (clause.Selector, Is.SameAs (_nodeWithResultSelector.GetResolvedResultSelector (ClauseGenerationContext)));
    }
  }
}
