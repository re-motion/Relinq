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
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
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
    public void GetSupportedMethods ()
    {
      Assert.That (
          SelectManyExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (() => Queryable.SelectMany<object, object[], object> (null, o => null, null)),
                  GetGenericMethodDefinition (() => Enumerable.SelectMany<object, object[], object> (null, o => null, null)),
                  GetGenericMethodDefinition (() => Queryable.SelectMany<object, object[]> (null, o => null)),
                  GetGenericMethodDefinition (() => Enumerable.SelectMany<object, object[]> (null, o => null)),
              }));
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
    public void GetResolvedResultSelector_WithoutClause ()
    {
      Assert.That (
          () => _nodeWithResultSelector.GetResolvedResultSelector (ClauseGenerationContext),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "Cannot retrieve an IQuerySource for the given SelectManyExpressionNode. "
                  + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both."));
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
