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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class SelectManyExpressionNodeTest : ExpressionNodeTestBase
  {
    private Expression<Func<int, bool>> _collectionSelector;
    private Expression<Func<int, int, bool>> _resultSelector;

    public override void SetUp ()
    {
      base.SetUp();

      _collectionSelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _resultSelector = ExpressionHelper.CreateLambdaExpression<int, int, bool> ((i, j) => i > j);
    }

    [Test]
    public void SupportedMethod_WithoutPosition ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.SelectMany (i => new[] { 1, 2, 3 }, (i, j) => new { i, j }));
      Assert.That (SelectManyExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void QuerySourceType ()
    {
      SelectManyExpressionNode node = ExpressionNodeObjectMother.CreateSelectMany (SourceNode);

      Assert.That (node.QuerySourceElementType, Is.SameAs (typeof (Student_Detail)));
    }

    [Test]
    public void Resolve_ReplacesParameter_WithProjection ()
    {
      var node = new SelectManyExpressionNode (
          CreateParseInfo(),
          _collectionSelector,
          ExpressionHelper.CreateLambdaExpression<int, int, AnonymousType> ((a, b) => new AnonymousType (a, b)));
      var clause = (FromClauseBase) node.CreateClause (SourceClause, ClauseGenerationContext);

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
      var node = new SelectManyExpressionNode (CreateParseInfo(), _collectionSelector, _resultSelector);
      var clause = (FromClauseBase) node.CreateClause (SourceClause, ClauseGenerationContext);
      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, new QuerySourceReferenceExpression (clause));

      var result = node.GetResolvedResultSelector (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException),
        ExpectedMessage = "Cannot resolve with a SelectManyExpressionNode for which no clause was "
                          +
                          "created. Be sure to call CreateClause before calling GetResolvedResultSelector, and pass in the same QuerySourceClauseMapping to both methods."
        )]
    public void GetResolvedResultSelector_WithoutClause ()
    {
      var node = new SelectManyExpressionNode (CreateParseInfo(), _collectionSelector, _resultSelector);
      node.GetResolvedResultSelector (ClauseGenerationContext);
    }

    [Test]
    public void GetResolvedCollectionSelector ()
    {
      var node = new SelectManyExpressionNode (CreateParseInfo(), _collectionSelector, _resultSelector);
      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = node.GetResolvedCollectionSelector (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void CreateClause ()
    {
      IClause previousClause = ExpressionHelper.CreateClause();
      var node = new SelectManyExpressionNode (CreateParseInfo (SourceNode, "j"), _collectionSelector, _resultSelector);

      var clause = (AdditionalFromClause) node.CreateClause (previousClause, ClauseGenerationContext);

      Assert.That (clause.Identifier.Name, Is.EqualTo ("j"));
      Assert.That (clause.Identifier.Type, Is.SameAs (typeof (int)));
      Assert.That (clause.FromExpression, Is.SameAs (node.GetResolvedCollectionSelector (ClauseGenerationContext)));
      Assert.That (clause.PreviousClause, Is.SameAs (previousClause));
    }

    [Test]
    public void CreateClause_WithMemberFromInFromExpression ()
    {
      var previousClause = ExpressionHelper.CreateMainFromClause (Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource());
      var collectionSelector = ExpressionHelper.CreateLambdaExpression<Student, IEnumerable<Student>> (s => s.Friends);

      var studentSource = new ConstantExpressionNode ("s", typeof (IQueryable<Student>), null);
      ClauseGenerationContext.ClauseMapping.AddMapping (studentSource, previousClause);
      var node = new SelectManyExpressionNode (CreateParseInfo (studentSource), collectionSelector, _resultSelector);

      var clause = (MemberFromClause) node.CreateClause (previousClause, ClauseGenerationContext);

      Assert.That (clause.Identifier.Name, Is.EqualTo ("j"));
      Assert.That (clause.Identifier.Type, Is.SameAs (typeof (int)));
      Assert.That (clause.FromExpression, Is.SameAs (node.GetResolvedCollectionSelector (ClauseGenerationContext)));
      Assert.That (clause.MemberExpression, Is.SameAs (node.GetResolvedCollectionSelector (ClauseGenerationContext)));
      Assert.That (clause.PreviousClause, Is.SameAs (previousClause));
    }

    [Test]
    public void CreateClause_WithSubQueryInFromExpression ()
    {
      IClause previousClause = ExpressionHelper.CreateClause();
      var subQueryExpression = new SubQueryExpression (ExpressionHelper.CreateQueryModel());
      
      var collectionSelector = Expression.Lambda (subQueryExpression, Expression.Parameter (typeof (int), "i"));
      var node = new SelectManyExpressionNode (CreateParseInfo(), collectionSelector, _resultSelector);

      var clause = (SubQueryFromClause) node.CreateClause (previousClause, ClauseGenerationContext);
      
      Assert.That (clause.Identifier.Name, Is.EqualTo ("j"));
      Assert.That (clause.Identifier.Type, Is.SameAs (typeof (int)));
      Assert.That (clause.SubQueryModel, Is.SameAs (subQueryExpression.QueryModel));
      Assert.That (clause.PreviousClause, Is.SameAs (previousClause));
    }

    [Test]
    public void CreateClause_AddsMapping ()
    {
      IClause previousClause = ExpressionHelper.CreateClause();
      var node = new SelectManyExpressionNode (CreateParseInfo (SourceNode, "j"), _collectionSelector, _resultSelector);
      var clause = (AdditionalFromClause) node.CreateClause (previousClause, ClauseGenerationContext);

      Assert.That (QuerySourceClauseMapping.GetClause (node), Is.SameAs (clause));
    }

    [Test]
    public void CreateParameterForOutput ()
    {
      var node = new SelectManyExpressionNode (
          CreateParseInfo(),
          ExpressionHelper.CreateLambdaExpression<Student, IEnumerable<int>> (y => y.Scores),
          ExpressionHelper.CreateLambdaExpression<Student, int, AnonymousType> ((s, i) => new AnonymousType()));

      var parameter = node.CreateParameterForOutput();

      Assert.That (parameter.Name, Is.EqualTo ("x"));
      Assert.That (parameter.Type, Is.SameAs (typeof (AnonymousType)));
    }
  }
}