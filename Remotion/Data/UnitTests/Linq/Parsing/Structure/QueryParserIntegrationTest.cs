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
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
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

      CheckConstantQuerySource (queryModel.MainFromClause.FromExpression, _querySource);

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
      CheckConstantQuerySource (mainFromClause.FromExpression, _querySource);

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
      CheckConstantQuerySource (mainFromClause.FromExpression, _industrialSectorQuerySource);

      var memberFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<IndustrialSector, IEnumerable<Student>> (memberFromClause.FromExpression, mainFromClause, sector => sector.Students);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, memberFromClause, s1 => s1);
    }

    [Test]
    public void GeneralSelectMany ()
    {
      var expression = FromTestQueryGenerator.CreateMultiFromQuery (_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, _querySource);
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
      var subQueryFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];

      var subQueryModel = ((SubQueryExpression) subQueryFromClause.FromExpression).QueryModel;
      var subQueryMainFromClause = subQueryModel.MainFromClause;
      Assert.That (subQueryMainFromClause.ItemName, Is.EqualTo ("s3"));
      CheckConstantQuerySource (subQueryMainFromClause.FromExpression, _querySource);

      var subQuerySelectClause = (SelectClause) subQueryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, Student> (subQuerySelectClause.Selector, subQueryMainFromClause, s3 => s3);
    }

    [Test]
    public void WhereSelectMany ()
    {
      var expression = MixedTestQueryGenerator.CreateReverseFromWhereQuery(_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, _querySource);

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
      CheckConstantQuerySource (mainFromClause.FromExpression, _querySource);

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
      CheckConstantQuerySource (mainFromClause.FromExpression, _querySource);

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
      CheckConstantQuerySource (mainFromClause.FromExpression, _querySource);
      
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

    [Test]
    public void SubQueryInMainFromClauseWithResultOperator ()
    {
      var query = from s in
                    (from sd1 in ExpressionHelper.CreateQuerySource_Detail () select sd1.Student).Take (5)
                  from sd in ExpressionHelper.CreateQuerySource_Detail ()
                  select new Tuple<Student, Student_Detail> ( s, sd );
      var expression = query.Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);
      Assert.That (queryModel.ResultType, Is.SameAs (typeof (IQueryable<Tuple<Student, Student_Detail>>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo("s"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (TakeResultOperator)));
      Assert.That (subQueryModel.ResultType, Is.SameAs (typeof (IQueryable<Student>)));
      
      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, Student_Detail, Tuple<Student, Student_Detail>> (
          selectClause.Selector, 
          mainFromClause, 
          additionalFromClause, 
          (s, sd) => new Tuple<Student, Student_Detail> (s, sd));
    }

    [Test]
    public void WhereClauseFollowingResultOperator ()
    {
      var query = (from s in ExpressionHelper.CreateQuerySource ()
                   select s).Distinct ().Where (x => x.ID > 0);
      
      var expression = query.Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);
      Assert.That (queryModel.ResultType, Is.SameAs (typeof (IQueryable<Student>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("x"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (DistinctResultOperator)));
      Assert.That (subQueryModel.ResultType, Is.SameAs (typeof (IQueryable<Student>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, mainFromClause, x => x.ID > 0);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedClause, Is.SameAs (mainFromClause));
    }

    [Test]
    public void PredicateFollowingResultOperator ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in ExpressionHelper.CreateQuerySource ()
                                                               select s).Distinct ().Count(x => x.ID > 0));

      var queryModel = _queryParser.GetParsedQuery (expression);
      Assert.That (queryModel.ResultType, Is.SameAs (typeof (int)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("x"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (DistinctResultOperator)));
      Assert.That (subQueryModel.ResultType, Is.SameAs (typeof (IQueryable<Student>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, mainFromClause, x => x.ID > 0);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedClause, Is.SameAs (mainFromClause));

      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (CountResultOperator)));
    }

    [Test]
    [Ignore ("TODO 1328")]
    public void GroupBy ()
    {
      var query = (from s in _querySource group s.ID by s.HasDog);
      
      var queryModel = _queryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.ResultType, Is.SameAs (typeof (IQueryable<IGrouping<bool, int>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, _querySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var groupClause = (GroupResultOperator) queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, bool> (groupClause.KeySelector, mainFromClause, s => s.HasDog);
      CheckResolvedExpression<Student, int> (groupClause.ElementSelector, mainFromClause, s => s.ID);
    }

    [Test]
    [Ignore ("TODO 1328")]
    public void GroupByWithoutElementSelector ()
    {
      var query = _querySource.GroupBy (s => s.HasDog);
      
      var queryModel = _queryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.ResultType, Is.SameAs (typeof (IQueryable<IGrouping<bool, Student>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, _querySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var groupClause = (GroupResultOperator) queryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, bool> (groupClause.KeySelector, mainFromClause, s => s.HasDog);
      CheckResolvedExpression<Student, Student> (groupClause.ElementSelector, mainFromClause, s => s);
    }

    [Test]
    [Ignore ("TODO 1328")]
    public void GroupIntoWithAggregate ()
    {
      var query = from s in _querySource 
                  group s.ID by s.HasDog 
                  into x 
                  where x.Count() > 0
                  select x;

      // equivalent to:
      //var query2 = from x in
      //               (from s in _querySource
      //                group s.ID by s.HasDog)
      //             where x.Count () > 0
      //             select x;

      // parsed as:
      //var query2 = from x in
      //               (from s in _querySource
      //                group s.ID by s.HasDog)
      //             where (from generated in x select generated).Count () > 0
      //             select x;

      var queryModel = _queryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.ResultType, Is.SameAs (typeof (IQueryable<IGrouping<bool, int>>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (IGrouping<bool, int>)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("x"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.SelectOrGroupClause, Is.InstanceOfType (typeof (GroupResultOperator)));
      var subQueryGroupClause = (GroupResultOperator) subQueryModel.SelectOrGroupClause;
      CheckResolvedExpression<Student, bool> (subQueryGroupClause.KeySelector, subQueryModel.MainFromClause, s => s.HasDog);
      CheckResolvedExpression<Student, int> (subQueryGroupClause.ElementSelector, subQueryModel.MainFromClause, s => s.ID);
      
      Assert.That (subQueryModel.ResultType, Is.SameAs (typeof (IQueryable<IGrouping<bool, int>>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      Assert.That (whereClause.Predicate, Is.InstanceOfType (typeof (BinaryExpression)));
      var predicateLeftSide = ((BinaryExpression) whereClause.Predicate).Left;
      Assert.That (predicateLeftSide, Is.InstanceOfType (typeof (SubQueryExpression)));
      var predicateSubQueryModel = ((SubQueryExpression) predicateLeftSide).QueryModel;
      Assert.That (predicateSubQueryModel.MainFromClause.ItemType, Is.SameAs (typeof (int)));
      Assert.That (predicateSubQueryModel.MainFromClause.ItemName, NUnit.Framework.SyntaxHelpers.Text.StartsWith ("<generated>"));
      Assert.That (((QuerySourceReferenceExpression) predicateSubQueryModel.MainFromClause.FromExpression).ReferencedClause, Is.SameAs (mainFromClause));
      Assert.That (((QuerySourceReferenceExpression) ((SelectClause) predicateSubQueryModel.SelectOrGroupClause).Selector).ReferencedClause, 
          Is.SameAs (predicateSubQueryModel.MainFromClause));
      Assert.That (predicateSubQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (CountResultOperator)));
      
      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedClause, Is.SameAs (mainFromClause));
    }

    [Test]
    [Ignore ("TODO 1328")]
    public void GroupByFollowedByWhere ()
    {
      var query = (from s in ExpressionHelper.CreateQuerySource ()
                   group s by s.HasDog).Where (g => g.Key);

      var queryModel = _queryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.ResultType, Is.SameAs (typeof (IQueryable<IGrouping<bool, Student>>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (IGrouping<bool, Student>)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("g"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.SelectOrGroupClause, Is.InstanceOfType (typeof (GroupResultOperator)));
      Assert.That (subQueryModel.ResultType, Is.SameAs (typeof (IQueryable<IGrouping<bool, Student>>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<IGrouping<bool, Student>, bool> (whereClause.Predicate, mainFromClause, g => g.Key);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedClause, Is.SameAs (mainFromClause));
    }

    private void CheckResolvedExpression<TParameter, TResult> (Expression expressionToCheck, FromClauseBase clauseToReference, Expression<Func<TParameter, TResult>> expectedUnresolvedExpression)
    {
      var expectedPredicate = ExpressionHelper.Resolve (clauseToReference, expectedUnresolvedExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedPredicate, expressionToCheck);
    }

    private void CheckResolvedExpression<TParameter1, TParameter2, TResult> (Expression expressionToCheck, FromClauseBase clauseToReference1, FromClauseBase clauseToReference2, Expression<Func<TParameter1, TParameter2, TResult>> expectedUnresolvedExpression)
    {
      var expectedPredicate = ExpressionHelper.Resolve (clauseToReference1, clauseToReference2, expectedUnresolvedExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedPredicate, expressionToCheck);
    }

    private void CheckConstantQuerySource (Expression expression, object expectedQuerySource)
    {
      Assert.That (expression, Is.InstanceOfType (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) expression).Value, Is.SameAs (expectedQuerySource));
    }
  }
}