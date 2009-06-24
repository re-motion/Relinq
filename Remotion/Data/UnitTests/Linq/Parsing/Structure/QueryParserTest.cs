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
using Remotion.Data.Linq.Clauses.ResultModifications;
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
      
      var newSelector = ((SelectClause) queryModel.SelectOrGroupClause).Selector;
      Assert.That (newSelector, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) newSelector).ReferencedClause, Is.SameAs (queryModel.MainFromClause));
    }

    [Test]
    public void CreateQueryModel_ConstantExpression_CreatesMainFromClause_WithGeneratedIdentifier ()
    {
      var value = new[] { 1, 2, 3 };
      var constantExpression = Expression.Constant (value);

      QueryModel queryModel = _queryParser.GetParsedQuery (constantExpression);

      Assert.That (queryModel.MainFromClause, Is.Not.Null);
      Assert.That (queryModel.MainFromClause.ItemName, Is.EqualTo ("<generated>_0"));
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
      Assert.That (queryModel.MainFromClause.ItemName, Is.EqualTo ("i"));
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
      
      var newSelector = ((SelectClause) queryModel.SelectOrGroupClause).Selector;
      Assert.That (newSelector, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) newSelector).ReferencedClause, Is.SameAs (queryModel.MainFromClause));
    }

    [Test]
    public void CreateQueryModel_WhereExpression_CreatesBodyClause ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Where (i => i > 5));

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);
      
      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (1));
      Assert.That (((WhereClause) queryModel.BodyClauses[0]).PreviousClause, Is.SameAs (queryModel.MainFromClause));
      
      var expectedPredicate = ExpressionHelper.Resolve<int, bool> (queryModel.MainFromClause, i => i > 5);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedPredicate, ((WhereClause) queryModel.BodyClauses[0]).Predicate);
    }

    [Test]
    public void CreateQueryModel_AppliesResultModifications ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Take (3).Count ());

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;

      Assert.That (selectClause.ResultModifications.Count, Is.EqualTo (2));
      Assert.That (selectClause.ResultModifications[0], Is.InstanceOfType (typeof (TakeResultModification)));
      Assert.That (selectClause.ResultModifications[1], Is.InstanceOfType (typeof (CountResultModification)));
    }

    [Test]
    public void CreateQueryModel_SubQueries_AreResolved ()
    {
      var expression = ExpressionHelper.MakeExpression (
           () => ExpressionHelper.CreateQuerySource ().Where (i => (from x in ExpressionHelper.CreateQuerySource () select i).Count () > 0));

      var result = _queryParser.GetParsedQuery (expression);
      var whereClause = (WhereClause) result.BodyClauses[0];
      var predicateBody = (BinaryExpression) whereClause.Predicate;
      var subQuerySelector = ((SelectClause)((SubQueryExpression) predicateBody.Left).QueryModel.SelectOrGroupClause).Selector;
      Assert.That (subQuerySelector, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression)subQuerySelector).ReferencedClause, Is.SameAs (result.MainFromClause));
    }

    [Test]
    public void CreateQueryModel_WithSelectClauseBeforeAnotherClause ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      // ReSharper disable RedundantAnonymousTypePropertyName
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (
          () => value.Select (i => new AnonymousType { a = i, b = i + 1 }).Where (trans => trans.a > 5));
      // ReSharper restore RedundantAnonymousTypePropertyName

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      Assert.That (queryModel.BodyClauses.Count (), Is.EqualTo (1));
      var whereClause = (WhereClause) (queryModel.BodyClauses[0]);
      var selectClause = (SelectClause) (queryModel.SelectOrGroupClause);

      var expectedPredicate = ExpressionHelper.Resolve<int, bool> (queryModel.MainFromClause, i => i > 5);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedPredicate, whereClause.Predicate);

      var expectedSelector = ExpressionHelper.Resolve<int, AnonymousType> (queryModel.MainFromClause, i => new AnonymousType { a = i, b = i + 1 });
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, selectClause.Selector);
    }

    [Test]
    public void IntegrationTest_CreateQueryModel_WithNonTrivialSelectClause_BeforeResultModification ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      // ReSharper disable RedundantAnonymousTypePropertyName
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Select (i => i + 1).Count ());
      // ReSharper restore RedundantAnonymousTypePropertyName

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);
      var selectClause = (SelectClause) (queryModel.SelectOrGroupClause);

      var expectedSelector = ExpressionHelper.Resolve<int, int> (queryModel.MainFromClause, i => i + 1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, selectClause.Selector);
    }

    [Test]
    public void IntegrationTest_CreateQueryModel_WithMultipleResultModifications ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      // ReSharper disable RedundantAnonymousTypePropertyName
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Select (i => i + 1).Take (1).Count ());
      // ReSharper restore RedundantAnonymousTypePropertyName

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);
      var selectClause = (SelectClause) (queryModel.SelectOrGroupClause);

      var expectedSelector = ExpressionHelper.Resolve<int, int> (queryModel.MainFromClause, i => i + 1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, selectClause.Selector);

      Assert.That (selectClause.ResultModifications[0], Is.InstanceOfType (typeof (TakeResultModification)));
      Assert.That (selectClause.ResultModifications[1], Is.InstanceOfType (typeof (CountResultModification)));
    }

    [Test]
    public void IntegrationTest_CreateQueryModel_WithResultModification_BeforeWhere ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      // ReSharper disable RedundantAnonymousTypePropertyName
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Select (i => i + 1).Take (1).Where (i => i > 5));
      // ReSharper restore RedundantAnonymousTypePropertyName

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);
      var selectClause = (SelectClause) (queryModel.SelectOrGroupClause);

      var expectedSelector = ExpressionHelper.Resolve<int, int> (queryModel.MainFromClause, i => i + 1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, selectClause.Selector);

      Assert.That (selectClause.ResultModifications.Count, Is.EqualTo (1));
      Assert.That (selectClause.ResultModifications[0], Is.InstanceOfType (typeof (TakeResultModification)));
      Assert.That (selectClause.PreviousClause, Is.InstanceOfType (typeof (WhereClause)));
    }
  }
}