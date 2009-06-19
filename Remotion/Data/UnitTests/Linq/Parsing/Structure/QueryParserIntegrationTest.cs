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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure
{
  [TestFixture]
  public class QueryParserIntegrationTest
  {
    private IQueryable<Student> _querySource;
    private QueryParser _queryParser;
    private IQueryable<IndustrialSector> _industrialSectorQuerySource;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();
      _industrialSectorQuerySource = ExpressionHelper.CreateQuerySource_IndustrialSector();
      _queryParser = new QueryParser();
    }

    [Test]
    public void SimpleSelect ()
    {
      var expression = SelectTestQueryGenerator.CreateSimpleQuery (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      Assert.That (selectClause.PreviousClause, Is.SameAs (queryModel.MainFromClause));
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void SimpleWhere ()
    {
      var expression = WhereTestQueryGenerator.CreateSimpleWhereQuery (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];

      Assert.That (queryModel.MainFromClause.Identifier.Name, Is.EqualTo ("s"));
      Assert.That (queryModel.MainFromClause.JoinClauses, Is.Empty);
      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      Assert.That (selectClause.PreviousClause, Is.SameAs (whereClause));
      Assert.That (whereClause.PreviousClause, Is.SameAs (queryModel.MainFromClause));

      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s => s.Last == "Garcia");
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void ThreeWheres ()
    {
      var expression = WhereTestQueryGenerator.CreateMultiWhereQuery (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      var whereClause1 = (WhereClause) queryModel.BodyClauses[0];
      var whereClause2 = (WhereClause) queryModel.BodyClauses[1];
      var whereClause3 = (WhereClause) queryModel.BodyClauses[2];

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (3));
      Assert.That (whereClause1.PreviousClause, Is.SameAs (queryModel.MainFromClause));
      Assert.That (whereClause2.PreviousClause, Is.SameAs (whereClause1));
      Assert.That (selectClause.PreviousClause, Is.SameAs (whereClause3));
      Assert.That (whereClause3.PreviousClause, Is.SameAs (whereClause2));

      CheckResolvedExpression<Student, bool> (whereClause1.Predicate, queryModel.MainFromClause, s => s.Last == "Garcia");
      CheckResolvedExpression<Student, bool> (whereClause2.Predicate, queryModel.MainFromClause, s => s.First == "Hugo");
      CheckResolvedExpression<Student, bool> (whereClause3.Predicate, queryModel.MainFromClause, s => s.ID > 100);
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void WhereWithDifferentComparisons ()
    {
      var expression = WhereTestQueryGenerator.CreateWhereQueryWithDifferentComparisons (_querySource).Expression;
      var navigator = new ExpressionTreeNavigator (expression);
      var queryModel = _queryParser.GetParsedQuery (expression);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      Assert.That (selectClause.PreviousClause, Is.SameAs (whereClause));
      Assert.That (whereClause.PreviousClause, Is.SameAs (queryModel.MainFromClause));
      
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, 
        queryModel.MainFromClause, s => s.First != "Garcia" && s.ID >5 && s.ID >= 6 && s.ID < 7 && s.ID <= 6 && s.ID == 6);
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);

      Assert.That (navigator.Arguments[0].Expression.NodeType, Is.EqualTo (queryModel.MainFromClause.QuerySource.NodeType));
      Assert.That (navigator.Arguments[0].Expression.Type, Is.EqualTo (queryModel.MainFromClause.QuerySource.Type));
    }

    [Test]
    public void MultiFromsAndWhere ()
    {
      var expression = MixedTestQueryGenerator.CreateMultiFromWhereQuery (_querySource, _querySource).Expression;
      var navigator = new ExpressionTreeNavigator (expression);
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      var selectClause = (SelectClause)queryModel.SelectOrGroupClause;
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      var whereClause = (WhereClause) queryModel.BodyClauses[1];

      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (additionalFromClause.PreviousClause, Is.SameAs (mainFromClause));
      Assert.That (whereClause.PreviousClause, Is.SameAs (additionalFromClause));
      Assert.That (selectClause.PreviousClause, Is.SameAs (whereClause));

      var selectMethodCall = navigator;
      var whereMethodCall = selectMethodCall.Arguments[0];
      var selectManyMethodCall = whereMethodCall.Arguments[0];

      Assert.That (((ConstantExpression) additionalFromClause.FromExpression).Value, Is.SameAs (_querySource));
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Last == "Garcia");
      Assert.That (mainFromClause.QuerySource.Type, Is.EqualTo (selectManyMethodCall.Arguments[0].Expression.Type));
      Assert.That (mainFromClause.JoinClauses.Count, Is.EqualTo (0));
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s1 => s1);
    }

    [Test]
    public void TwoFromsWithMemberAccess ()
    {
      var expression = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySource (_industrialSectorQuerySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      var memberFromClause = (MemberFromClause) queryModel.BodyClauses[0];
      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;

      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (memberFromClause.PreviousClause, Is.SameAs (mainFromClause));
      Assert.That (selectClause.PreviousClause, Is.SameAs (memberFromClause));

      Assert.That (((ConstantExpression) mainFromClause.QuerySource).Value, Is.SameAs (_industrialSectorQuerySource));
      Assert.That (mainFromClause.JoinClauses.Count, Is.EqualTo (0));
      CheckResolvedExpression<IndustrialSector, IEnumerable<Student>> (memberFromClause.MemberExpression, mainFromClause, sector => sector.Students);
      CheckResolvedExpression<Student, Student> (selectClause.Selector, memberFromClause, s1 => s1);
    }

    [Test]
    public void GeneralSelectMany ()
    {
      var expression = FromTestQueryGenerator.CreateMultiFromQuery (_querySource, _querySource).Expression;
      var navigator = new ExpressionTreeNavigator (expression);
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      
      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (mainFromClause.Identifier.Name, Is.EqualTo ("s1"));
      Assert.That (mainFromClause.Identifier.Type, Is.SameAs(typeof(Student)));
      Assert.That (queryModel.BodyClauses[0], Is.InstanceOfType (typeof (AdditionalFromClause)));
      Assert.That (selectClause.PreviousClause, Is.SameAs (additionalFromClause));
      Assert.That (additionalFromClause.Identifier.Name, Is.EqualTo ("s2"));
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s1 => s1);
      Assert.That (mainFromClause.QuerySource.Type, Is.EqualTo(navigator.Arguments[0].Expression.Type));
    }

    [Test]
    [Ignore ("TODO 1222: Should work again after integrating SubQueryFromClause")]
    public void SimpleSubQueryInAdditionalFromClause ()
    {
      var expression = SubQueryTestQueryGenerator.CreateSimpleSubQueryInAdditionalFromClause (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (1));
      var subQueryFromClause = queryModel.BodyClauses[0] as SubQueryFromClause;
      Assert.IsNotNull (subQueryFromClause);

      Assert.That (subQueryFromClause.SubQueryModel, Is.Not.Null);
      Assert.That (subQueryFromClause.SubQueryModel.ParentQuery, Is.SameAs (queryModel));
      Assert.That (subQueryFromClause.SubQueryModel.MainFromClause, Is.Not.Null);

      var subQueryMainFromClause = subQueryFromClause.SubQueryModel.MainFromClause;
      Assert.That (subQueryMainFromClause.Identifier.Name, Is.EqualTo ("s3"));
      Assert.That (((ConstantExpression)subQueryMainFromClause.QuerySource).Value, Is.SameAs(_querySource));

      var subQuerySelectClause = (SelectClause) subQueryFromClause.SubQueryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (subQuerySelectClause.Selector, subQueryMainFromClause, s3 => s3);
    }

    [Test]
    public void WhereSelectMany ()
    {
      var expression = MixedTestQueryGenerator.CreateReverseFromWhereQuery(_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);

      var mainFromClause = queryModel.MainFromClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[1];

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (2));
      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.SameAs (additionalFromClause));
      Assert.That (whereClause.PreviousClause, Is.SameAs (mainFromClause));

      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Last == "Garcia");
      Assert.That (mainFromClause.QuerySource.Type, Is.EqualTo (navigator.Arguments[0].Arguments[0].Expression.Type));
      Assert.That (additionalFromClause.Identifier.Name, Is.EqualTo ("s2"));
      Assert.That (((ConstantExpression) additionalFromClause.FromExpression).Value, Is.SameAs (_querySource));
    }

    [Test]
    public void SelectMany_InSelectMany ()
    {
      var expression = MixedTestQueryGenerator.CreateThreeFromWhereQuery (_querySource, _querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);
      
      var additionalFromClause1 = (AdditionalFromClause) queryModel.BodyClauses[0];
      var whereClause = (WhereClause) queryModel.BodyClauses[1];
      var additionalFromClause2 = (AdditionalFromClause) queryModel.BodyClauses[2];
      var mainFromClause = queryModel.MainFromClause;

      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (additionalFromClause1.PreviousClause, Is.SameAs (mainFromClause));
      Assert.That (whereClause.PreviousClause, Is.SameAs (additionalFromClause1));
      Assert.That (additionalFromClause2.PreviousClause, Is.SameAs (whereClause));
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.SameAs (additionalFromClause2));
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.First == "Hugo");
      Assert.That (mainFromClause.QuerySource.Type, Is.EqualTo (navigator.Arguments[0].Arguments[0].Arguments[0].Expression.Type));
      Assert.That (additionalFromClause1.Identifier.Name, Is.EqualTo ("s2"));
      Assert.That (additionalFromClause2.Identifier.Name, Is.EqualTo ("s3"));
    }

    [Test]
    public void WhereAndSelectMany ()
    {
      var expression = MixedTestQueryGenerator.CreateReverseFromWhereQuery (_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      
      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (mainFromClause.Identifier.Name, Is.EqualTo ("s1"));
      Assert.That (mainFromClause.JoinClauses.Count, Is.EqualTo (0));

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (2));
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Last == "Garcia");
      Assert.That (queryModel.BodyClauses.Last(), Is.InstanceOfType (typeof (AdditionalFromClause)));
      var selectClause = (SelectClause)queryModel.SelectOrGroupClause;
      Assert.That (selectClause, Is.Not.Null);
    }

    [Test]
    public void WhereAndSelectMannyWithProjection ()
    {
      var expression = MixedTestQueryGenerator.CreateReverseFromWhereQueryWithProjection (_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];

      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (mainFromClause.Identifier.Name, Is.EqualTo ("s1"));
      Assert.That (mainFromClause.JoinClauses.Count, Is.EqualTo (0));
      
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Last == "Garcia");
      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (2));
      Assert.That (queryModel.BodyClauses.Last (), Is.InstanceOfType (typeof (AdditionalFromClause)));
      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      Assert.That (selectClause, Is.Not.Null);
    }

    [Test]
    public void Let ()
    {
      var expression = LetTestQueryGenerator.CreateSimpleLetClause (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      var selectClause = ((SelectClause) queryModel.SelectOrGroupClause);

      Assert.That (queryModel.BodyClauses.Count (), Is.EqualTo (0));
      CheckResolvedExpression<Student, string> (selectClause.Selector, mainFromClause, s => s.First + s.Last);
    }

    [Test]
    public void OrderByAndThenBy ()
    {
      var expression = OrderByTestQueryGenerator.CreateOrderByQueryWithOrderByAndThenBy (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);

      var orderByClause = (OrderByClause) queryModel.BodyClauses[0];
      var ordering1 = orderByClause.OrderingList[0];
      var ordering2 = orderByClause.OrderingList[1];
      var ordering3 = orderByClause.OrderingList[2];

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      var mainFromClause = queryModel.MainFromClause;

      Assert.That (ordering1.OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
      Assert.That (ordering2.OrderingDirection, Is.EqualTo (OrderingDirection.Desc));
      Assert.That (ordering3.OrderingDirection, Is.EqualTo (OrderingDirection.Asc));

      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (mainFromClause.Identifier.Name, Is.EqualTo ("s"));
      Assert.That (orderByClause.OrderingList.Count, Is.EqualTo (3));
      Assert.That (orderByClause.PreviousClause, Is.SameAs (mainFromClause));
      Assert.That (selectClause.PreviousClause, Is.SameAs (orderByClause));

      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
      Assert.That (mainFromClause.QuerySource.NodeType, Is.EqualTo (navigator.Arguments[0].Arguments[0].Arguments[0].Expression.NodeType));
    }

    [Test]
    public void MultipleOrderBys ()
    {
      var expression = OrderByTestQueryGenerator.CreateOrderByQueryWithMultipleOrderBys (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);


      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.PreviousClause, Is.Null);
      var orderByClause1 = (OrderByClause) queryModel.BodyClauses[0];
      Assert.That (orderByClause1.OrderingList.Count, Is.EqualTo (3));
      var orderByClause2 = (OrderByClause) queryModel.BodyClauses[1];
      Assert.That (orderByClause2.OrderingList.Count, Is.EqualTo (1));

      Assert.That (orderByClause1.PreviousClause, Is.SameAs (mainFromClause));
      Assert.That (orderByClause2.PreviousClause, Is.SameAs (orderByClause1));
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.SameAs (orderByClause2));
      Assert.That (mainFromClause.QuerySource.NodeType, Is.EqualTo (navigator.Arguments[0].Arguments[0].Arguments[0].Arguments[0].Expression.NodeType));
      Assert.That (mainFromClause.QuerySource.Type, Is.EqualTo (navigator.Arguments[0].Arguments[0].Arguments[0].Arguments[0].Expression.Type));
    }

    [Test]
    public void OrderByAndWhere ()
    {
      var expression = MixedTestQueryGenerator.CreateOrderByWithWhereCondition (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      var orderByClause = (OrderByClause) queryModel.BodyClauses[1];
      var mainFromClause = queryModel.MainFromClause;

      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (whereClause.PreviousClause, Is.SameAs (mainFromClause));
      Assert.That (orderByClause.PreviousClause, Is.SameAs (whereClause));
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.SameAs (orderByClause));
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.First == "Garcia");
    }

    [Test]
    public void MultiFromsWithOrderBy ()
    {
      var expression = MixedTestQueryGenerator.CreateMultiFromWhereOrderByQuery (_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);
      
      var memberFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      var whereClause = (WhereClause) queryModel.BodyClauses[1];
      var orderByClause = (OrderByClause) queryModel.BodyClauses[2];

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      Assert.That (memberFromClause.PreviousClause, Is.SameAs (queryModel.MainFromClause));
      Assert.That (whereClause.PreviousClause, Is.SameAs (memberFromClause));
      Assert.That (orderByClause.PreviousClause, Is.SameAs (whereClause));
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.SameAs (orderByClause));
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Last == "Garcia");
    }

    private void CheckResolvedExpression<TParameter, TResult> (Expression expressionToCheck, FromClauseBase clauseToReference, Expression<Func<TParameter, TResult>> expectedUnresolvedExpression)
    {
      var expectedPredicate = ExpressionHelper.Resolve (clauseToReference, expectedUnresolvedExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedPredicate, expressionToCheck);
    }
  }
}