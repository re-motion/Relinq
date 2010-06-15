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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestUtilities;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class ResultOperatorQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void SubQueryInMainFromClauseWithResultOperator ()
    {
      var query = from s in
                      (from sd1 in DetailQuerySource select sd1.Cook).Take (5)
                  from sd in DetailQuerySource
                  select new Tuple<Cook, Kitchen> ( s, sd );
      var expression = query.Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Tuple<Cook, Kitchen>>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo("s"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (TakeResultOperator)));
      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));
      
      var selectClause = queryModel.SelectClause;
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, Kitchen, Tuple<Cook, Kitchen>> (
          selectClause.Selector, 
          mainFromClause, 
          additionalFromClause, 
          (s, sd) => new Tuple<Cook, Kitchen> (s, sd));
    }

    [Test]
    public void WhereClauseFollowingResultOperator ()
    {
      var query = (from s in ExpressionHelper.CreateCookQueryable ()
                   select s).Distinct ().Where (x => x.ID > 0);
      
      var expression = query.Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("x"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (DistinctResultOperator)));
      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, mainFromClause, x => x.ID > 0);

      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }

    [Test]
    public void PredicateFollowingResultOperator ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in ExpressionHelper.CreateCookQueryable ()
                                                               select s).Distinct ().Count(x => x.ID > 0));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (int)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("x"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (DistinctResultOperator)));
      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, mainFromClause, x => x.ID > 0);

      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));

      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (CountResultOperator)));
    }

    [Test]
    public void All_FollowingGroupBy ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in ExpressionHelper.CreateCookQueryable ()
                                                               select s).GroupBy (c => c.Name, c => c.ID).All (group => group.Key != null));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Console.WriteLine (queryModel);
    }

    [Test]
    public void TakeWithBackReference ()
    {
      var query =
          from s in QuerySource
          from s1 in s.Assistants.Take (s.ID)
          select s1;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var mainFromClause = queryModel.MainFromClause;
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];

      var subQueryModel = ((SubQueryExpression) additionalFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (TakeResultOperator)));

      var takeResultOperator = (TakeResultOperator) subQueryModel.ResultOperators[0];
      CheckResolvedExpression<Cook, int> (takeResultOperator.Count, mainFromClause, s => s.ID);

      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));
    }

    [Test]
    public void Average ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                          select s.ID).Average());

      var queryModel = QueryParser.GetParsedQuery (expression);

      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (double)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (AverageResultOperator)));
      
      CheckResolvedExpression<Cook, int> (queryModel.SelectClause.Selector, mainFromClause, s => s.ID);
    }

    [Test]
    public void LongCount ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s.ID).LongCount ());

      var queryModel = QueryParser.GetParsedQuery (expression);

      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (long)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (LongCountResultOperator)));

      CheckResolvedExpression<Cook, int> (queryModel.SelectClause.Selector, mainFromClause, s => s.ID);
    }

    [Test]
    public void ListCount ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s.Holidays.Count));

      var queryModel = QueryParser.GetParsedQuery (expression);

      var selectClause = queryModel.SelectClause;
      Assert.That (selectClause.Selector, Is.InstanceOfType (typeof (SubQueryExpression)));

      var subQueryModel = ((SubQueryExpression) selectClause.Selector).QueryModel;
      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (CountResultOperator)));
    }

    [Test]
    public void ListCount_NotSubQuery ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select new List<int>().Count));

      var queryModel = QueryParser.GetParsedQuery (expression);

      var selectClause = queryModel.SelectClause;
      Assert.That (selectClause.Selector, Is.InstanceOfType (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) selectClause.Selector).Value, Is.EqualTo (0));
    }

    [Test]
    public void ArrayListCount ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s.Courses.Count));

      var queryModel = QueryParser.GetParsedQuery (expression);

      var selectClause = queryModel.SelectClause;
      Assert.That (selectClause.Selector, Is.InstanceOfType (typeof (SubQueryExpression)));

      var subQueryModel = ((SubQueryExpression) selectClause.Selector).QueryModel;
      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (CountResultOperator)));
    }

    [Test]
    public void ArrayLength ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s.IllnessDays.Length));

      var queryModel = QueryParser.GetParsedQuery (expression);

      var selectClause = queryModel.SelectClause;
      Assert.That (selectClause.Selector, Is.InstanceOfType (typeof (SubQueryExpression)));

      var subQueryModel = ((SubQueryExpression) selectClause.Selector).QueryModel;
      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (CountResultOperator)));
    }

    [Test]
    public void ArrayLongLength ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s.IllnessDays.LongLength));

      var queryModel = QueryParser.GetParsedQuery (expression);

      var selectClause = queryModel.SelectClause;
      Assert.That (selectClause.Selector, Is.InstanceOfType (typeof (SubQueryExpression)));

      var subQueryModel = ((SubQueryExpression) selectClause.Selector).QueryModel;
      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (LongCountResultOperator)));
    }

    [Test]
    public void Skip ()
    {
      var query = (from s in ExpressionHelper.CreateCookQueryable() 
                   select s.ID).Skip (1);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<int>)));

      var mainFromClause = queryModel.MainFromClause;

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (SkipResultOperator)));
      var skipResultOperator = (SkipResultOperator) queryModel.ResultOperators[0];
      CheckResolvedExpression<int,int> (skipResultOperator.Count, mainFromClause, i => 1);
    }

    [Test]
    public void Reverse ()
    {
      var query = (from s in ExpressionHelper.CreateCookQueryable ()
                   select s).Reverse ();

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var mainFromClause = queryModel.MainFromClause;

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (ReverseResultOperator)));
      var skipResultOperator = (ReverseResultOperator) queryModel.ResultOperators[0];
      Assert.That (skipResultOperator, Is.Not.Null);

      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }

    [Test]
    public void Except ()
    {
      IEnumerable<Cook> students = new[] { new Cook() };
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Except (students));
      
      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var mainFromClause = queryModel.MainFromClause;

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (ExceptResultOperator)));
      Assert.That (((ExceptResultOperator) queryModel.ResultOperators[0]).GetConstantSource2(), Is.SameAs (students));
      
      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }

    [Test]
    public void Intersect ()
    {
      IEnumerable<Cook> students = new[] { new Cook () };
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Intersect (students));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var mainFromClause = queryModel.MainFromClause;

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (IntersectResultOperator)));
      Assert.That (((IntersectResultOperator) queryModel.ResultOperators[0]).GetConstantSource2 (), Is.SameAs (students));
      
      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }

    [Test]
    public void Union ()
    {
      IEnumerable<Cook> students = new[] { new Cook () };
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Union (students));


      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var mainFromClause = queryModel.MainFromClause;

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (UnionResultOperator)));
      Assert.That (((UnionResultOperator) queryModel.ResultOperators[0]).GetConstantSource2 (), Is.SameAs (students));
      
      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }

    [Test]
    public void DefaultIfEmpty ()
    {
      var student = new Cook ();
      var query = (from s in ExpressionHelper.CreateCookQueryable ()
                   select s).DefaultIfEmpty (student);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (DefaultIfEmptyResultOperator)));
      Assert.That (((DefaultIfEmptyResultOperator) queryModel.ResultOperators[0]).GetConstantOptionalDefaultValue(), Is.SameAs (student));
    }

    [Test]
    public void Cast ()
    {
      var query = (from s in ExpressionHelper.CreateCookQueryable()
                   select s.ID).Cast<double>();
      
      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<double>)));

      var castResultOperator = (CastResultOperator) queryModel.ResultOperators[0];
      Assert.That (castResultOperator.CastItemType, Is.SameAs (typeof (double)));

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (CastResultOperator)));
    }

    [Test]
    public void Contains ()
    {
      var student = new Cook ();
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Contains (student));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (bool)));

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (ContainsResultOperator)));
      Assert.That (((ContainsResultOperator) queryModel.ResultOperators[0]).GetConstantItem<Cook>(), Is.SameAs (student));
    }

    [Test]
    public void All ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).All (s => s.IsFullTimeCook));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (bool)));

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (AllResultOperator)));

      CheckResolvedExpression<Cook, bool> (((AllResultOperator) queryModel.ResultOperators[0]).Predicate, queryModel.MainFromClause, s => s.IsFullTimeCook);
    }

    [Test]
    public void Any ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Any ());

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (bool)));

      CheckConstantQuerySource (queryModel.MainFromClause.FromExpression, QuerySource);
      
      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (AnyResultOperator)));
    }

    [Test]
    public void Any_WithPredicate ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Any (s => s.IsFullTimeCook));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (bool)));

      CheckConstantQuerySource (queryModel.MainFromClause.FromExpression, QuerySource);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (1));
      Assert.That (queryModel.BodyClauses[0], Is.InstanceOfType (typeof (WhereClause)));

      CheckResolvedExpression<Cook, bool> (((WhereClause) queryModel.BodyClauses[0]).Predicate, queryModel.MainFromClause, s => s.IsFullTimeCook);
      
      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (AnyResultOperator)));
    }

    [Test]
    public void Aggregate_NoSeed ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s.Name).Aggregate ((allNames, name) => allNames + " " + name));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (string)));

      CheckConstantQuerySource (queryModel.MainFromClause.FromExpression, QuerySource);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (AggregateResultOperator)));

      var resultOperator = (AggregateResultOperator) queryModel.ResultOperators[0];

      var expectedFunc = ExpressionHelper.ResolveLambdaParameter<string, Cook, string> (
          1, 
          queryModel.MainFromClause,
          (allNames, student) => allNames + " " + student.Name);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedFunc, resultOperator.Func);
    }

    [Test]
    public void Aggregate_Seed_NoResultSelector ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Aggregate (0, (totalIDs, s) => totalIDs + s.ID));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (int)));

      CheckConstantQuerySource (queryModel.MainFromClause.FromExpression, QuerySource);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (AggregateFromSeedResultOperator)));

      var resultOperator = (AggregateFromSeedResultOperator) queryModel.ResultOperators[0];
      Assert.That (resultOperator.GetConstantSeed<int> (), Is.EqualTo (0));

      var expectedFunc = ExpressionHelper.ResolveLambdaParameter<int, Cook, int> (
          1,
          queryModel.MainFromClause,
          (totalIDs, s) => totalIDs + s.ID);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedFunc, resultOperator.Func);

      Assert.That (resultOperator.OptionalResultSelector, Is.Null);
    }

    [Test]
    public void Aggregate_Seed_WithResultSelector ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Aggregate (0, (totalIDs, s) => totalIDs + s.ID, totalIDs => totalIDs.ToString ()));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (string)));

      CheckConstantQuerySource (queryModel.MainFromClause.FromExpression, QuerySource);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (AggregateFromSeedResultOperator)));

      var resultOperator = (AggregateFromSeedResultOperator) queryModel.ResultOperators[0];
      Assert.That (resultOperator.GetConstantSeed<int> (), Is.EqualTo (0));

      var expectedFunc = ExpressionHelper.ResolveLambdaParameter<int, Cook, int> (
          1,
          queryModel.MainFromClause,
          (totalIDs, s) => totalIDs + s.ID);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedFunc, resultOperator.Func);

      var expectedResultSelector = ExpressionHelper.CreateLambdaExpression<int, string> (totalIDs => totalIDs.ToString ());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResultSelector, resultOperator.OptionalResultSelector);
    }
  }
}
