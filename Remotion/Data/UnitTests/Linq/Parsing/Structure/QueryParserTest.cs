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
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Structure;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure
{
  [TestFixture]
  public class QueryParserTest
  {
    private QueryParser _queryParser;

    [SetUp]
    public void SetUp ()
    {
      _queryParser = new QueryParser();
    }

    [Test]
    public void Initialization_Default ()
    {
      Assert.That (_queryParser.ExpressionTreeParser.NodeTypeRegistry.Count, Is.GreaterThan (0));
    }

    [Test]
    public void Initialization_InjectExpressionTreeParser ()
    {
      var expressionTreeParser = new ExpressionTreeParser (new MethodCallExpressionNodeTypeRegistry());
      var queryParser = new QueryParser (expressionTreeParser);

      Assert.That (queryParser.ExpressionTreeParser, Is.SameAs (expressionTreeParser));
    }

    [Test]
    public void CreateQueryModel_ConstantExpression_CreatesSelectClause ()
    {
      var value = new[] { 1, 2, 3 };
      var constantExpression = Expression.Constant (value);

      QueryModel queryModel = _queryParser.GetParsedQuery(constantExpression);

      Assert.That (queryModel.SelectOrGroupClause, Is.Not.Null);
      
      var newLegacySelector = ((SelectClause) queryModel.SelectOrGroupClause).LegacySelector;
      Assert.That (newLegacySelector.Body, Is.SameAs (newLegacySelector.Parameters[0]));
      Assert.That (newLegacySelector.Parameters[0].Type, Is.SameAs (typeof (int)));
      Assert.That (newLegacySelector.Parameters[0].Name, Is.EqualTo ("<generated>_0"));

      var newSelector = ((SelectClause) queryModel.SelectOrGroupClause).Selector;
      Assert.That (newSelector, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) newSelector).ReferencedClause, Is.SameAs (queryModel.MainFromClause));
    }

    [Test]
    public void CreateQueryModel_SetsExpressionTree ()
    {
      var value = new[] { 1, 2, 3 };
      var constantExpression = Expression.Constant (value);

      QueryModel queryModel = _queryParser.GetParsedQuery (constantExpression);
      Assert.That (queryModel.GetExpressionTree(), Is.SameAs (constantExpression));
    }

    [Test]
    public void CreateQueryModel_SetsExpressionTree_EvenWithBodyClauses ()
    {
      var queryExpression = ExpressionHelper.MakeExpression (() => ExpressionHelper.CreateQuerySource().Where (s => s.ID > 5));

      QueryModel queryModel = _queryParser.GetParsedQuery (queryExpression);
      Assert.That (queryModel.GetExpressionTree (), Is.SameAs (queryExpression));
    }

    [Test]
    public void CreateQueryModel_ConstantExpression_CreatesMainFromClause_WithGeneratedIdentifier ()
    {
      var value = new[] { 1, 2, 3 };
      var constantExpression = Expression.Constant (value);

      QueryModel queryModel = _queryParser.GetParsedQuery (constantExpression);

      Assert.That (queryModel.MainFromClause, Is.Not.Null);
      Assert.That (queryModel.MainFromClause.Identifier.Name, Is.EqualTo ("<generated>_0"));
      Assert.That (((ConstantExpression) queryModel.MainFromClause.QuerySource).Value, Is.SameAs (value));
    }

    [Test]
    public void CreateQueryModel_SelectExpression_UsesSelectClauseFromNode ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Select (i => i.ToString ()));

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      Assert.That (queryModel.SelectOrGroupClause, Is.Not.Null);
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.SameAs (queryModel.MainFromClause));
      Assert.That (((SelectClause) queryModel.SelectOrGroupClause).LegacySelector, 
          Is.SameAs (((UnaryExpression) expressionTree.Arguments[1]).Operand));

      Assert.That (((SelectClause) queryModel.SelectOrGroupClause).Selector, Is.InstanceOfType (typeof (MethodCallExpression)));
      Assert.That (((MethodCallExpression)((SelectClause) queryModel.SelectOrGroupClause).Selector).Method.Name, Is.EqualTo("ToString"));
    }

    [Test]
    public void CreateQueryModel_CorrectFromIdentifier ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Select (i => i.ToString ()));

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      Assert.That (queryModel.MainFromClause, Is.Not.Null);
      Assert.That (queryModel.MainFromClause.Identifier.Name, Is.EqualTo ("i"));
    }

    [Test]
    public void CreateQueryModel_WhereExpression_CreatesSelectClause ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (
          () => value.Where (i => i > 5));

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      Assert.That (queryModel.SelectOrGroupClause, Is.Not.Null);
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.InstanceOfType (typeof (WhereClause)));
      
      var newLegacySelector = ((SelectClause) queryModel.SelectOrGroupClause).LegacySelector;
      Assert.That (newLegacySelector.Body, Is.SameAs (newLegacySelector.Parameters[0]));
      Assert.That (newLegacySelector.Parameters[0].Type, Is.SameAs (typeof (int)));
      Assert.That (newLegacySelector.Parameters[0].Name, Is.EqualTo ("i"));

      var newSelector = ((SelectClause) queryModel.SelectOrGroupClause).Selector;
      Assert.That (newSelector, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) newSelector).ReferencedClause, Is.SameAs (queryModel.MainFromClause));
    }

    [Test]
    public void CreateQueryModel_WhereExpression_CreatesBodyClause ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (
          () => value.Where (i => i > 5));

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);
      
      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (1));
      Assert.That (((WhereClause) queryModel.BodyClauses[0]).PreviousClause, Is.SameAs (queryModel.MainFromClause));
      Assert.That (((WhereClause) queryModel.BodyClauses[0]).LegacyPredicate, Is.SameAs(((UnaryExpression)expressionTree.Arguments[1]).Operand));
    }

    [Test]
    public void CreateQueryModel_WithSelectClauseBeforeAnotherClause ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (
          () => value.Select (i => new { i = i, j = i.ToString() }).Where (trans => trans.i > 5));

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      Assert.That (queryModel.BodyClauses[0], Is.InstanceOfType (typeof (LetClause)));
      var letClause = (LetClause) queryModel.BodyClauses[0];

      Assert.That (letClause.PreviousClause, Is.SameAs (queryModel.MainFromClause));
      Assert.That (letClause.Identifier.Name, Is.EqualTo ("j"));
      Assert.That (letClause.Identifier.Type, Is.SameAs (typeof (string)));

      Assert.That (queryModel.BodyClauses[1].PreviousClause, Is.SameAs (letClause));

      var expectedLetExpression = ExpressionHelper.MakeExpression<int, string> (i => i.ToString ());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedLetExpression, letClause.Expression);

      var expectedLetProjection = ((UnaryExpression) ((MethodCallExpression) expressionTree.Arguments[0]).Arguments[1]).Operand;
      Assert.That (letClause.ProjectionExpression, Is.SameAs (expectedLetProjection));
    }

    [Test]
    public void CreateQueryModel_SubQueries_AreAssociatedWithParentQueries ()
    {
      var expression = ExpressionHelper.MakeExpression (
           () => ExpressionHelper.CreateQuerySource ().Where (i => (
               from x in ExpressionHelper.CreateQuerySource ()
               select x).Count () > 0));

      var result = _queryParser.GetParsedQuery (expression);
      var whereClause = (WhereClause) result.BodyClauses[0];
      var predicateBody = (BinaryExpression) whereClause.LegacyPredicate.Body;
      Assert.That (((SubQueryExpression)predicateBody.Left).QueryModel.ParentQuery, Is.SameAs (result));
    }

  }
}