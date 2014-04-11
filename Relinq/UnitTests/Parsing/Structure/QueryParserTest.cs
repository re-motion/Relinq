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
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure
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
      var expressionTreeParser = _queryParser.ExpressionTreeParser;
      Assert.That (expressionTreeParser.NodeTypeProvider, Is.TypeOf (typeof (CompoundNodeTypeProvider)));
      var nodeTypeProviders = ((CompoundNodeTypeProvider) expressionTreeParser.NodeTypeProvider).InnerProviders;
      Assert.That (nodeTypeProviders[0], Is.TypeOf (typeof (MethodInfoBasedNodeTypeRegistry)));
      Assert.That (nodeTypeProviders[1], Is.TypeOf (typeof (MethodNameBasedNodeTypeRegistry)));

      Assert.That (expressionTreeParser.Processor, Is.TypeOf (typeof (CompoundExpressionTreeProcessor)));
      var processingSteps = ((CompoundExpressionTreeProcessor) expressionTreeParser.Processor).InnerProcessors;
      Assert.That (processingSteps.Count, Is.EqualTo (2));
      Assert.That (processingSteps[0], Is.TypeOf (typeof (PartialEvaluatingExpressionTreeProcessor)));
      Assert.That (processingSteps[1], Is.TypeOf (typeof (TransformingExpressionTreeProcessor)));
      Assert.That (((TransformingExpressionTreeProcessor) processingSteps[1]).Provider, Is.TypeOf (typeof (ExpressionTransformerRegistry)));
      
      var expressionTransformerRegistry =
          ((ExpressionTransformerRegistry) ((TransformingExpressionTreeProcessor) processingSteps[1]).Provider);
      Assert.That (
          expressionTransformerRegistry.RegisteredTransformerCount,
          Is.EqualTo (ExpressionTransformerRegistry.CreateDefault ().RegisteredTransformerCount));
    }

    [Test]
    public void Initialization_InjectExpressionTreeParser ()
    {
      var expressionTreeParser = new ExpressionTreeParser (new MethodInfoBasedNodeTypeRegistry(), new NullExpressionTreeProcessor());
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
      Assert.That (newSelector, Is.InstanceOf (typeof (QuerySourceReferenceExpression)));
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
      Assert.That (queryModel.SelectClause.Selector, Is.InstanceOf (typeof (MethodCallExpression)));
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
      Assert.That (newSelector, Is.InstanceOf (typeof (QuerySourceReferenceExpression)));
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
      Assert.That (queryModel.BodyClauses[0], Is.InstanceOf (typeof (WhereClause)));
      Assert.That (queryModel.BodyClauses[1], Is.InstanceOf (typeof (OrderByClause)));
    }

    [Test]
    public void CreateQueryModel_AppliesResultOperators ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Distinct().Count ());

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (2));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (DistinctResultOperator)));
      Assert.That (queryModel.ResultOperators[1], Is.InstanceOf (typeof (CountResultOperator)));
    }

    [Test]
    public void CreateQueryModel_CollectsWhereClausesOfResultOperators ()
    {
      IQueryable<int> value = new[] { 1, 2, 3 }.AsQueryable ();
      var expressionTree = (MethodCallExpression) ExpressionHelper.MakeExpression (() => value.Count (i => i > 5));

      QueryModel queryModel = _queryParser.GetParsedQuery (expressionTree);

      Assert.That (queryModel.BodyClauses[0], Is.InstanceOf (typeof (WhereClause)));
    }

    [Test]
    public void CreateQueryModel_SubQueries_AreResolved ()
    {
      var expression = ExpressionHelper.MakeExpression (
           () => ExpressionHelper.CreateQueryable<Cook> ().Where (i => (from x in ExpressionHelper.CreateQueryable<Cook> () select i).Count () > 0));

      var result = _queryParser.GetParsedQuery (expression);
      var whereClause = (WhereClause) result.BodyClauses[0];
      var predicateBody = (BinaryExpression) whereClause.Predicate;
      var subQuerySelector = ((SubQueryExpression) predicateBody.Left).QueryModel.SelectClause.Selector;
      Assert.That (subQuerySelector, Is.InstanceOf (typeof (QuerySourceReferenceExpression)));
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

      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (DistinctResultOperator)));
      Assert.That (queryModel.ResultOperators[1], Is.InstanceOf (typeof (CountResultOperator)));
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
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (DistinctResultOperator)));
    }
  }
}
