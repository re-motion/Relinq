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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.ExpressionTreeProcessingSteps;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure
{
  [TestFixture]
  public class QueryParserTest
  {
    private QueryParser _queryParser;

    [SetUp]
    public void SetUp ()
    {
      _queryParser = QueryParser.CreateDefault();
    }

    [Test]
    public void Initialization_Default ()
    {
      Assert.That (
          _queryParser.NodeTypeRegistry.RegisteredMethodInfoCount, 
          Is.EqualTo (MethodCallExpressionNodeTypeRegistry.CreateDefault().RegisteredMethodInfoCount));

      Assert.That (_queryParser.ProcessingSteps.Count, Is.EqualTo (2));
      Assert.That (_queryParser.ProcessingSteps[0], Is.TypeOf (typeof (PartialEvaluationStep)));
      Assert.That (_queryParser.ProcessingSteps[1], Is.TypeOf (typeof (ExpressionTransformationStep)));
      Assert.That (
          ((ExpressionTransformationStep) _queryParser.ProcessingSteps[1]).Provider,
          Is.TypeOf (typeof (ExpressionTransformerRegistry)));
      
      var expressionTransformerRegistry = 
          ((ExpressionTransformerRegistry) ((ExpressionTransformationStep) _queryParser.ProcessingSteps[1]).Provider);
      Assert.That (
          expressionTransformerRegistry.RegisteredTransformerCount,
          Is.EqualTo (ExpressionTransformerRegistry.CreateDefault ().RegisteredTransformerCount));
    }

    [Test]
    public void Initialization_InjectExpressionTreeParser ()
    {
      var expressionTreeParser = new ExpressionTreeParser (new MethodCallExpressionNodeTypeRegistry(), new IExpressionTreeProcessingStep[0]);
      var queryParser = new QueryParser (expressionTreeParser);

      Assert.That (queryParser.ExpressionTreeParser, Is.SameAs (expressionTreeParser));
    }

    [Test]
    public void CreateQueryModel_ConstantExpression_CreatesSelectClause ()
    {
      var value = new[] { 1, 2, 3 };
      var constantExpression = Expression.Constant (value);

      QueryModel queryModel = _queryParser.GetParsedQuery(constantExpression);

      Assert.That (queryModel.SelectClause, Is.Not.Null);
      
      var newSelector = queryModel.SelectClause.Selector;
      Assert.That (newSelector, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) newSelector).ReferencedQuerySource, Is.SameAs (queryModel.MainFromClause));
    }

    [Test]
    public void CreateQueryModel_ConstantExpression_CreatesMainFromClause_WithGeneratedIdentifier ()
    {
      var value = new[] { 1, 2, 3 };
      var constantExpression = Expression.Constant (value);

      QueryModel queryModel = _queryParser.GetParsedQuery (constantExpression);

      Assert.That (queryModel.MainFromClause, Is.Not.Null);
      Assert.That (queryModel.MainFromClause.ItemName, Is.EqualTo ("<generated>_0"));
      Assert.That (((ConstantExpression) queryModel.MainFromClause.FromExpression).Value, Is.SameAs (value));
    }

    [Test]
    public void CreateQueryModel_SelectExpression_UsesSelectClauseFromNode ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Select (i => i.ToString ()));

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      Assert.That (queryModel.SelectClause, Is.Not.Null);
      Assert.That (queryModel.SelectClause.Selector, Is.InstanceOfType (typeof (MethodCallExpression)));
      Assert.That (((MethodCallExpression)queryModel.SelectClause.Selector).Method.Name, Is.EqualTo("ToString"));
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

      Assert.That (queryModel.SelectClause, Is.Not.Null);

      var newSelector = queryModel.SelectClause.Selector;
      Assert.That (newSelector, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) newSelector).ReferencedQuerySource, Is.SameAs (queryModel.MainFromClause));
    }

    [Test]
    public void CreateQueryModel_WhereExpression_CreatesBodyClause ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Where (i => i > 5));

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);
      
      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (1));
      
      var expectedPredicate = ExpressionHelper.Resolve<int, bool> (queryModel.MainFromClause, i => i > 5);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedPredicate, ((WhereClause) queryModel.BodyClauses[0]).Predicate);
    }

    [Test]
    public void CreateQueryModel_OrderOfBodyClauses ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Where (i => i > 5).OrderBy (i => i));

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (2));
      Assert.That (queryModel.BodyClauses[0], Is.InstanceOfType (typeof (WhereClause)));
      Assert.That (queryModel.BodyClauses[1], Is.InstanceOfType (typeof (OrderByClause)));
    }

    [Test]
    public void CreateQueryModel_AppliesResultOperators ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Distinct().Count ());

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (2));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (DistinctResultOperator)));
      Assert.That (queryModel.ResultOperators[1], Is.InstanceOfType (typeof (CountResultOperator)));
    }

    [Test]
    public void CreateQueryModel_CollectsWhereClausesOfResultOperators ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Count (i => i > 5));

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      Assert.That (queryModel.BodyClauses[0], Is.InstanceOfType (typeof (WhereClause)));
    }

    [Test]
    public void CreateQueryModel_SubQueries_AreResolved ()
    {
      var expression = ExpressionHelper.MakeExpression (
           () => ExpressionHelper.CreateCookQueryable ().Where (i => (from x in ExpressionHelper.CreateCookQueryable () select i).Count () > 0));

      var result = _queryParser.GetParsedQuery (expression);
      var whereClause = (WhereClause) result.BodyClauses[0];
      var predicateBody = (BinaryExpression) whereClause.Predicate;
      var subQuerySelector = ((SubQueryExpression) predicateBody.Left).QueryModel.SelectClause.Selector;
      Assert.That (subQuerySelector, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression)subQuerySelector).ReferencedQuerySource, Is.SameAs (result.MainFromClause));
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
      var selectClause = queryModel.SelectClause;

      var expectedPredicate = ExpressionHelper.Resolve<int, bool> (queryModel.MainFromClause, i => i > 5);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedPredicate, whereClause.Predicate);

      var expectedSelector = ExpressionHelper.Resolve<int, AnonymousType> (queryModel.MainFromClause, i => new AnonymousType { a = i, b = i + 1 });
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, selectClause.Selector);
    }

    [Test]
    public void IntegrationTest_CreateQueryModel_WithNonTrivialSelectClause_BeforeResultOperator ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      // ReSharper disable RedundantAnonymousTypePropertyName
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Select (i => i + 1).Count ());
      // ReSharper restore RedundantAnonymousTypePropertyName

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);
      var selectClause = queryModel.SelectClause;

      var expectedSelector = ExpressionHelper.Resolve<int, int> (queryModel.MainFromClause, i => i + 1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, selectClause.Selector);
    }

    [Test]
    public void IntegrationTest_CreateQueryModel_WithMultipleResultOperators ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      // ReSharper disable RedundantAnonymousTypePropertyName
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Select (i => i + 1).Distinct().Count ());
      // ReSharper restore RedundantAnonymousTypePropertyName

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);
      var selectClause = queryModel.SelectClause;

      var expectedSelector = ExpressionHelper.Resolve<int, int> (queryModel.MainFromClause, i => i + 1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, selectClause.Selector);

      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (DistinctResultOperator)));
      Assert.That (queryModel.ResultOperators[1], Is.InstanceOfType (typeof (CountResultOperator)));
    }

    [Test]
    public void IntegrationTest_CreateQueryModel_WithResultOperator_BeforeWhere ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      // ReSharper disable RedundantAnonymousTypePropertyName
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Select (i => i + 1).Distinct().Where (i => i > 5));
      // ReSharper restore RedundantAnonymousTypePropertyName

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);
      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (0));

      var selectClause = queryModel.SelectClause;
      var expectedSelector = ExpressionHelper.Resolve<int, int> (queryModel.MainFromClause, i => i);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, selectClause.Selector);

      var subQueryModel = ((SubQueryExpression) queryModel.MainFromClause.FromExpression).QueryModel;
      var subSelectClause = subQueryModel.SelectClause;

      var expectedSubSelector = ExpressionHelper.Resolve<int, int> (subQueryModel.MainFromClause, i => i + 1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSubSelector, subSelectClause.Selector);

      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (DistinctResultOperator)));
    }
  }
}
