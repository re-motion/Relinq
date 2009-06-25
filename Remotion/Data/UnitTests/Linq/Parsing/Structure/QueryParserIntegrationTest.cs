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
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void SimpleWhere ()
    {
      var expression = WhereTestQueryGenerator.CreateSimpleWhereQuery (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      Assert.That (queryModel.MainFromClause.ItemName, Is.EqualTo ("s"));
      Assert.That (queryModel.MainFromClause.JoinClauses, Is.Empty);

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s => s.Last == "Garcia");

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void ThreeWheres ()
    {
      var expression = WhereTestQueryGenerator.CreateMultiWhereQuery (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (3));

      var whereClause1 = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause1.Predicate, queryModel.MainFromClause, s => s.Last == "Garcia");

      var whereClause2 = (WhereClause) queryModel.BodyClauses[1];
      CheckResolvedExpression<Student, bool> (whereClause2.Predicate, queryModel.MainFromClause, s => s.First == "Hugo");

      var whereClause3 = (WhereClause) queryModel.BodyClauses[2];
      CheckResolvedExpression<Student, bool> (whereClause3.Predicate, queryModel.MainFromClause, s => s.ID > 100);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void WhereWithDifferentComparisons ()
    {
      var expression = WhereTestQueryGenerator.CreateWhereQueryWithDifferentComparisons (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      CheckConstantQuerySource (queryModel.MainFromClause.QuerySource, _querySource);

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, 
        queryModel.MainFromClause, s => s.First != "Garcia" && s.ID > 5 && s.ID >= 6 && s.ID < 7 && s.ID <= 6 && s.ID == 6);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void MultiFromsAndWhere ()
    {
      var expression = MixedTestQueryGenerator.CreateMultiFromWhereQuery (_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.QuerySource, _querySource);

      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      CheckConstantQuerySource (additionalFromClause.FromExpression, _querySource);

      var whereClause = (WhereClause) queryModel.BodyClauses[1];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Last == "Garcia");

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s1 => s1);
    }

    [Test]
    public void TwoFromsWithMemberAccess ()
    {
      var expression = FromTestQueryGenerator.CreateFromQueryWithMemberQuerySource (_industrialSectorQuerySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.QuerySource, _industrialSectorQuerySource);

      var memberFromClause = (MemberFromClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<IndustrialSector, IEnumerable<Student>> (memberFromClause.MemberExpression, mainFromClause, sector => sector.Students);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, memberFromClause, s1 => s1);
    }

    [Test]
    public void GeneralSelectMany ()
    {
      var expression = FromTestQueryGenerator.CreateMultiFromQuery (_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.QuerySource, _querySource);
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s1"));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Student)));

      Assert.That (queryModel.BodyClauses[0], Is.InstanceOfType (typeof (AdditionalFromClause)));
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      Assert.That (additionalFromClause.ItemName, Is.EqualTo ("s2"));
      CheckConstantQuerySource (additionalFromClause.FromExpression, _querySource);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s1 => s1);
    }

    [Test]
    public void SimpleSubQueryInAdditionalFromClause ()
    {
      var expression = SubQueryTestQueryGenerator.CreateSimpleSubQueryInAdditionalFromClause (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (1));
      var subQueryFromClause = queryModel.BodyClauses[0] as SubQueryFromClause;
      Assert.IsNotNull (subQueryFromClause);

      Assert.That (subQueryFromClause.SubQueryModel, Is.Not.Null);
      Assert.That (subQueryFromClause.SubQueryModel.MainFromClause, Is.Not.Null);

      var subQueryMainFromClause = subQueryFromClause.SubQueryModel.MainFromClause;
      Assert.That (subQueryMainFromClause.ItemName, Is.EqualTo ("s3"));
      CheckConstantQuerySource (subQueryMainFromClause.QuerySource, _querySource);

      var subQuerySelectClause = (SelectClause) subQueryFromClause.SubQueryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (subQuerySelectClause.Selector, subQueryMainFromClause, s3 => s3);
    }

    [Test]
    public void WhereSelectMany ()
    {
      var expression = MixedTestQueryGenerator.CreateReverseFromWhereQuery(_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.QuerySource, _querySource);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (2));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Last == "Garcia");

      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[1];
      Assert.That (additionalFromClause.ItemName, Is.EqualTo ("s2"));
      CheckConstantQuerySource (additionalFromClause.FromExpression, _querySource);
    }

    [Test]
    public void SelectMany_InSelectMany ()
    {
      var expression = MixedTestQueryGenerator.CreateThreeFromWhereQuery (_querySource, _querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.QuerySource, _querySource);

      var additionalFromClause1 = (AdditionalFromClause) queryModel.BodyClauses[0];
      Assert.That (additionalFromClause1.ItemName, Is.EqualTo ("s2"));
      
      var whereClause = (WhereClause) queryModel.BodyClauses[1];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.First == "Hugo");

      var additionalFromClause2 = (AdditionalFromClause) queryModel.BodyClauses[2];
      Assert.That (additionalFromClause2.ItemName, Is.EqualTo ("s3"));
    }

    [Test]
    public void WhereAndSelectMany ()
    {
      var expression = MixedTestQueryGenerator.CreateReverseFromWhereQuery (_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s1"));

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (2));
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, mainFromClause, s1 => s1.Last == "Garcia");
      Assert.That (queryModel.BodyClauses.Last (), Is.InstanceOfType (typeof (AdditionalFromClause)));
      
      var selectClause = (SelectClause)queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, mainFromClause, s1 => s1);
    }

    [Test]
    public void WhereAndSelectMannyWithProjection ()
    {
      var expression = MixedTestQueryGenerator.CreateReverseFromWhereQueryWithProjection (_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s1"));
      Assert.That (mainFromClause.JoinClauses.Count, Is.EqualTo (0));

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (2));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Last == "Garcia");

      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[1];
      CheckConstantQuerySource (additionalFromClause.FromExpression, _querySource);
      
      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, string> (selectClause.Selector, (AdditionalFromClause) queryModel.BodyClauses.Last(), s2 => s2.Last);
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

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));
      CheckConstantQuerySource (mainFromClause.QuerySource, _querySource);

      var orderByClause = (OrderByClause) queryModel.BodyClauses[0];
      Assert.That (orderByClause.Orderings.Count, Is.EqualTo (3));

      var ordering1 = orderByClause.Orderings[0];
      Assert.That (ordering1.OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
      CheckResolvedExpression<Student, string> (ordering1.Expression, mainFromClause, s => s.First);

      var ordering2 = orderByClause.Orderings[1];
      Assert.That (ordering2.OrderingDirection, Is.EqualTo (OrderingDirection.Desc));
      CheckResolvedExpression<Student, string> (ordering2.Expression, mainFromClause, s => s.Last);

      var ordering3 = orderByClause.Orderings[2];
      Assert.That (ordering3.OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
      CheckResolvedExpression<Student, List<int>> (ordering3.Expression, mainFromClause, s => s.Scores);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void MultipleOrderBys ()
    {
      var expression = OrderByTestQueryGenerator.CreateOrderByQueryWithMultipleOrderBys (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.QuerySource, _querySource);
      
      var orderByClause1 = (OrderByClause) queryModel.BodyClauses[0];
      Assert.That (orderByClause1.Orderings.Count, Is.EqualTo (3));

      var orderByClause2 = (OrderByClause) queryModel.BodyClauses[1];
      Assert.That (orderByClause2.Orderings.Count, Is.EqualTo (1));
    }

    [Test]
    public void OrderByAndWhere ()
    {
      var expression = MixedTestQueryGenerator.CreateOrderByWithWhereCondition (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.First == "Garcia");

      var orderByClause = (OrderByClause) queryModel.BodyClauses[1];
      CheckResolvedExpression<Student, string> (orderByClause.Orderings[0].Expression, queryModel.MainFromClause, s1 => s1.First);
    }

    [Test]
    public void MultiFromsWithOrderBy ()
    {
      var expression = MixedTestQueryGenerator.CreateMultiFromWhereOrderByQuery (_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);
      
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      CheckConstantQuerySource (additionalFromClause.FromExpression, _querySource);

      var whereClause = (WhereClause) queryModel.BodyClauses[1];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Last == "Garcia");

      var orderByClause = (OrderByClause) queryModel.BodyClauses[2];
      Assert.That (orderByClause.Orderings[0].OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
      CheckResolvedExpression<Student, string> (orderByClause.Orderings[0].Expression, queryModel.MainFromClause, s1 => s1.First);
      Assert.That (orderByClause.Orderings[1].OrderingDirection, Is.EqualTo (OrderingDirection.Desc));
      CheckResolvedExpression<Student, string> (orderByClause.Orderings[1].Expression, additionalFromClause, s2 => s2.Last);
    }

    private void CheckResolvedExpression<TParameter, TResult> (Expression expressionToCheck, FromClauseBase clauseToReference, Expression<Func<TParameter, TResult>> expectedUnresolvedExpression)
    {
      var expectedPredicate = ExpressionHelper.Resolve (clauseToReference, expectedUnresolvedExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedPredicate, expressionToCheck);
    }

    private void CheckConstantQuerySource (Expression expression, object expectedQuerySource)
    {
      Assert.That (expression, Is.InstanceOfType (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) expression).Value, Is.SameAs (expectedQuerySource));
    }
  }
}