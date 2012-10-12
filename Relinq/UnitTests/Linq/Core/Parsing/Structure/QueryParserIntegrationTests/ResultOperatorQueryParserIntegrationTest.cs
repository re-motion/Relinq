// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.Core.TestUtilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
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
                  select new NonTransformedTuple<Cook, Kitchen> ( s, sd );
      var expression = query.Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<NonTransformedTuple<Cook, Kitchen>>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOf (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo("s"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (TakeResultOperator)));
      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));
      
      var selectClause = queryModel.SelectClause;
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, Kitchen, NonTransformedTuple<Cook, Kitchen>> (
          selectClause.Selector, 
          mainFromClause, 
          additionalFromClause, 
          (s, sd) => new NonTransformedTuple<Cook, Kitchen> (s, sd));
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
      Assert.That (mainFromClause.FromExpression, Is.InstanceOf (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("x"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (DistinctResultOperator)));
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
      Assert.That (mainFromClause.FromExpression, Is.InstanceOf (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("x"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (DistinctResultOperator)));
      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, mainFromClause, x => x.ID > 0);

      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));

      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (CountResultOperator)));
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
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (TakeResultOperator)));

      var takeResultOperator = (TakeResultOperator) subQueryModel.ResultOperators[0];
      CheckResolvedExpression<Cook, int> (takeResultOperator.Count, mainFromClause, s => s.ID);

      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));
    }

    [Test]
    public void TakeWithSubQuery ()
    {
      var query =
          from s in QuerySource
          from s1 in s.Assistants.Take (s.Assistants.Count() / 2)
          select s1;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var mainFromClause = queryModel.MainFromClause;
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];

      var subQueryModel = ((SubQueryExpression) additionalFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (TakeResultOperator)));

      var takeResultOperator = (TakeResultOperator) subQueryModel.ResultOperators[0];
      Assert.That (takeResultOperator.Count.NodeType, Is.EqualTo (ExpressionType.Divide));
      var takeDivideExpression = (BinaryExpression) takeResultOperator.Count;
      Assert.That (takeDivideExpression.Left, Is.TypeOf (typeof (SubQueryExpression)));

      var takeSubQueryModel = ((SubQueryExpression) takeDivideExpression.Left).QueryModel;
      CheckResolvedExpression<Cook, IEnumerable<Cook>> (takeSubQueryModel.MainFromClause.FromExpression, mainFromClause, s => s.Assistants);
      CheckResolvedExpression<Cook, Cook> (takeSubQueryModel.SelectClause.Selector, takeSubQueryModel.MainFromClause, a => a);
      Assert.That (takeSubQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (takeSubQueryModel.ResultOperators[0], Is.TypeOf (typeof (CountResultOperator)));
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
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (AverageResultOperator)));
      
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
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (LongCountResultOperator)));

      CheckResolvedExpression<Cook, int> (queryModel.SelectClause.Selector, mainFromClause, s => s.ID);
    }

    [Test]
    public void ListCount ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s.Holidays.Count));

      var queryModel = QueryParser.GetParsedQuery (expression);

      var selectClause = queryModel.SelectClause;
      Assert.That (selectClause.Selector, Is.InstanceOf (typeof (SubQueryExpression)));

      var subQueryModel = ((SubQueryExpression) selectClause.Selector).QueryModel;
      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (CountResultOperator)));
    }

    [Test]
    public void ListCount_NotSubQuery ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select new List<int>().Count));

      var queryModel = QueryParser.GetParsedQuery (expression);

      var selectClause = queryModel.SelectClause;
      Assert.That (selectClause.Selector, Is.InstanceOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) selectClause.Selector).Value, Is.EqualTo (0));
    }

    [Test]
    public void ArrayListCount ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s.Courses.Count));

      var queryModel = QueryParser.GetParsedQuery (expression);

      var selectClause = queryModel.SelectClause;
      Assert.That (selectClause.Selector, Is.InstanceOf (typeof (SubQueryExpression)));

      var subQueryModel = ((SubQueryExpression) selectClause.Selector).QueryModel;
      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (CountResultOperator)));
    }

    [Test]
    public void ArrayLength ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s.IllnessDays.Length));

      var queryModel = QueryParser.GetParsedQuery (expression);

      var selectClause = queryModel.SelectClause;
      Assert.That (selectClause.Selector, Is.InstanceOf (typeof (SubQueryExpression)));

      var subQueryModel = ((SubQueryExpression) selectClause.Selector).QueryModel;
      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (CountResultOperator)));
    }

    [Test]
    public void ArrayLongLength ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s.IllnessDays.LongLength));

      var queryModel = QueryParser.GetParsedQuery (expression);

      var selectClause = queryModel.SelectClause;
      Assert.That (selectClause.Selector, Is.InstanceOf (typeof (SubQueryExpression)));

      var subQueryModel = ((SubQueryExpression) selectClause.Selector).QueryModel;
      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (LongCountResultOperator)));
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
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (SkipResultOperator)));
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
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (ReverseResultOperator)));
      var skipResultOperator = (ReverseResultOperator) queryModel.ResultOperators[0];
      Assert.That (skipResultOperator, Is.Not.Null);

      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }

    [Test]
    public void Except ()
    {
      IEnumerable<Cook> students = new[] { new Cook() };
      var expression = ExpressionHelper.MakeExpression (() => (from c in QuerySource
                                                               select c).Except (students));
      
      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var mainFromClause = queryModel.MainFromClause;

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (ExceptResultOperator)));
      var exceptResultOperator = ((ExceptResultOperator) queryModel.ResultOperators[0]);
      Assert.That (exceptResultOperator.GetConstantSource2(), Is.SameAs (students));
      
      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));

      var outputDataInfo = (StreamedSequenceInfo) queryModel.GetOutputDataInfo ();
      Assert.That (outputDataInfo.DataType, Is.SameAs (typeof (IQueryable<Cook>)));
      CheckResolvedExpression<Cook, Cook> (outputDataInfo.ItemExpression, mainFromClause, c => c);
    }

    [Test]
    public void Except_FollowedByAll ()
    {
      IEnumerable<Cook> cooks = new[] { new Cook () };
      var expression = ExpressionHelper.MakeExpression (() => (from c in QuerySource
                                                               select c)
                                                               .Except (cooks)
                                                               .All (c => c.IsFullTimeCook));


      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (bool)));

      Assert.That (queryModel.ResultOperators, Has.Count.EqualTo (2));
      Assert.That (queryModel.ResultOperators[0], Is.TypeOf<ExceptResultOperator> ());
      Assert.That (queryModel.ResultOperators[1], Is.TypeOf<AllResultOperator> ());

      var allResultOperator = (AllResultOperator) queryModel.ResultOperators[1];
      CheckResolvedExpression<Cook, bool> (allResultOperator.Predicate, queryModel.MainFromClause, c => c.IsFullTimeCook);
    }

    [Test]
    public void Intersect ()
    {
      IEnumerable<Cook> students = new[] { new Cook () };
      var expression = ExpressionHelper.MakeExpression (() => (from c in QuerySource
                                                               select c).Intersect (students));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var mainFromClause = queryModel.MainFromClause;

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (IntersectResultOperator)));
      var intersectResultOperator = ((IntersectResultOperator) queryModel.ResultOperators[0]);
      Assert.That (intersectResultOperator.GetConstantSource2 (), Is.SameAs (students));
      
      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));

      var outputDataInfo = (StreamedSequenceInfo) queryModel.GetOutputDataInfo ();
      Assert.That (outputDataInfo.DataType, Is.SameAs (typeof (IQueryable<Cook>)));
      CheckResolvedExpression<Cook, Cook> (outputDataInfo.ItemExpression, mainFromClause, c => c);
    }

    [Test]
    public void Intersect_FollowedByAll ()
    {
      IEnumerable<Cook> cooks = new[] { new Cook () };
      var expression = ExpressionHelper.MakeExpression (() => (from c in QuerySource
                                                               select c)
                                                               .Intersect (cooks)
                                                               .All (c => c.IsFullTimeCook));


      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (bool)));

      Assert.That (queryModel.ResultOperators, Has.Count.EqualTo (2));
      Assert.That (queryModel.ResultOperators[0], Is.TypeOf<IntersectResultOperator> ());
      Assert.That (queryModel.ResultOperators[1], Is.TypeOf<AllResultOperator> ());

      var allResultOperator = (AllResultOperator) queryModel.ResultOperators[1];
      CheckResolvedExpression<Cook, bool> (allResultOperator.Predicate, queryModel.MainFromClause, c => c.IsFullTimeCook);
    }

    [Test]
    public void Union ()
    {
      IEnumerable<Cook> students = new[] { new Cook () };
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Union (students));

      var queryModel = QueryParser.GetParsedQuery (expression);
      var mainFromClause = queryModel.MainFromClause;

      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (UnionResultOperator)));
      var unionResultOperator = ((UnionResultOperator) queryModel.ResultOperators[0]);
      Assert.That (unionResultOperator.GetConstantSource2 (), Is.SameAs (students));
      
      var outputDataInfo = (StreamedSequenceInfo) queryModel.GetOutputDataInfo();
      Assert.That (outputDataInfo.DataType, Is.SameAs (typeof (IQueryable<Cook>)));
      CheckResolvedExpression<Cook, Cook> (outputDataInfo.ItemExpression, unionResultOperator, c => c);
    }

    [Test]
    public void Union_FollowedByAll ()
    {
      IEnumerable<Cook> cooks = new[] { new Cook () };
      var expression = ExpressionHelper.MakeExpression (() => (from c in QuerySource
                                                               select c)
                                                               .Union (cooks)
                                                               .All (c => c.IsFullTimeCook));


      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (bool)));

      Assert.That (queryModel.ResultOperators, Has.Count.EqualTo (2));
      Assert.That (queryModel.ResultOperators[0], Is.TypeOf<UnionResultOperator> ());
      Assert.That (queryModel.ResultOperators[1], Is.TypeOf<AllResultOperator> ());

      var unionResultOperator = (UnionResultOperator) queryModel.ResultOperators[0];
      var allResultOperator = (AllResultOperator) queryModel.ResultOperators[1];

      CheckResolvedExpression<Cook, bool> (allResultOperator.Predicate, unionResultOperator, c => c.IsFullTimeCook);
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
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (DefaultIfEmptyResultOperator)));
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
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (CastResultOperator)));
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
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (ContainsResultOperator)));
      Assert.That (((ContainsResultOperator) queryModel.ResultOperators[0]).GetConstantItem<Cook>(), Is.SameAs (student));
    }

    [Test]
    public void Contains_TypeImplementingIList ()
    {
      var list = new List<Cook> ();
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select list.Contains (s)));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<bool>)));

      Assert.That (queryModel.SelectClause.Selector, Is.TypeOf (typeof (SubQueryExpression)));
      var subQueryModel = ((SubQueryExpression) queryModel.SelectClause.Selector).QueryModel;

      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (ContainsResultOperator)));

      var exoectedItemExpression = new QuerySourceReferenceExpression (queryModel.MainFromClause);
      ExpressionTreeComparer.CheckAreEqualTrees (exoectedItemExpression, ((ContainsResultOperator) subQueryModel.ResultOperators[0]).Item);
    }

    [Test]
    public void Contains_GenericICollection ()
    {
      ICollection<Cook> list = new List<Cook> ();
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select list.Contains (s)));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<bool>)));

      Assert.That (queryModel.SelectClause.Selector, Is.TypeOf (typeof (SubQueryExpression)));
      var subQueryModel = ((SubQueryExpression) queryModel.SelectClause.Selector).QueryModel;

      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (ContainsResultOperator)));

      var exoectedItemExpression = new QuerySourceReferenceExpression (queryModel.MainFromClause);
      ExpressionTreeComparer.CheckAreEqualTrees (exoectedItemExpression, ((ContainsResultOperator) subQueryModel.ResultOperators[0]).Item);
    }

    [Test]
    public void Contains_GenericIList ()
    {
      IList<Cook> list = new List<Cook> ();
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select list.Contains (s)));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<bool>)));

      Assert.That (queryModel.SelectClause.Selector, Is.TypeOf (typeof (SubQueryExpression)));
      var subQueryModel = ((SubQueryExpression) queryModel.SelectClause.Selector).QueryModel;

      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (ContainsResultOperator)));

      var exoectedItemExpression = new QuerySourceReferenceExpression (queryModel.MainFromClause);
      ExpressionTreeComparer.CheckAreEqualTrees (exoectedItemExpression, ((ContainsResultOperator) subQueryModel.ResultOperators[0]).Item);
    }

    [Test]
    public void Contains_NonGenericIList ()
    {
      IList list = new List<Cook> ();
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select list.Contains (s)));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<bool>)));

      Assert.That (queryModel.SelectClause.Selector, Is.TypeOf (typeof (SubQueryExpression)));
      var subQueryModel = ((SubQueryExpression) queryModel.SelectClause.Selector).QueryModel;

      Assert.That (subQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (ContainsResultOperator)));

      var exoectedItemExpression = new QuerySourceReferenceExpression (queryModel.MainFromClause);
      ExpressionTreeComparer.CheckAreEqualTrees (exoectedItemExpression, ((ContainsResultOperator) subQueryModel.ResultOperators[0]).Item);
    }

    [Test]
    public void ContainsWithBackReference ()
    {
      var query =
          from s in QuerySource
          where s.Assistants.Contains (s)
          select s;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var mainFromClause = queryModel.MainFromClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];

      var subQueryModel = ((SubQueryExpression) whereClause.Predicate).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (ContainsResultOperator)));

      var containsResultOperator = (ContainsResultOperator) subQueryModel.ResultOperators[0];
      CheckResolvedExpression<Cook, Cook> (containsResultOperator.Item, mainFromClause, s => s);
    }

    [Test]
    public void ContainsWithSubQuery ()
    {
      var query =
          from s in QuerySource
          where s.Assistants.Contains (s.Assistants.First())
          select s;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var mainFromClause = queryModel.MainFromClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];

      var subQueryModel = ((SubQueryExpression) whereClause.Predicate).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (ContainsResultOperator)));

      var containsResultOperator = (ContainsResultOperator) subQueryModel.ResultOperators[0];
      Assert.That (containsResultOperator.Item, Is.TypeOf (typeof (SubQueryExpression)));

      var containsSubQueryModel = ((SubQueryExpression) containsResultOperator.Item).QueryModel;
      CheckResolvedExpression<Cook, IEnumerable<Cook>> (containsSubQueryModel.MainFromClause.FromExpression, mainFromClause, s => s.Assistants);
      CheckResolvedExpression<Cook, Cook> (containsSubQueryModel.SelectClause.Selector, containsSubQueryModel.MainFromClause, a => a);
      Assert.That (containsSubQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (containsSubQueryModel.ResultOperators[0], Is.TypeOf (typeof (FirstResultOperator)));
    }

    [Test]
    public void Contains_IDictionary_IsNotTranslatedAsAContainsResultOperator ()
    {
      IDictionary dictionary = new Dictionary<string, int> ();
      var query = from c in QuerySource
                  where dictionary.Contains (c.Name)
                  select c;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var whereClause = (WhereClause) queryModel.BodyClauses.Single();
      Assert.That (whereClause.Predicate, Is.Not.InstanceOf<SubQueryExpression> ());
      Assert.That (whereClause.Predicate, Is.InstanceOf<MethodCallExpression> ());
    }

    [Test]
    public void Contains_IDictionary_Implementation_IsNotTranslatedAsAContainsResultOperator ()
    {
      var dictionary = new Hashtable();
      var query = from c in QuerySource
                  where dictionary.Contains (c.Name)
                  select c;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var whereClause = (WhereClause) queryModel.BodyClauses.Single ();
      Assert.That (whereClause.Predicate, Is.Not.InstanceOf<SubQueryExpression> ());
      Assert.That (whereClause.Predicate, Is.InstanceOf<MethodCallExpression> ());
    }

    [Test]
    public void Contains_Enumerable_OnDictionary_IsTranslatedAsAContainsResultOperator ()
    {
      var dictionary = new Dictionary<string, int>();
      var query = from c in QuerySource
                  where dictionary.Contains (new KeyValuePair<string, int> (c.Name, c.ID))
                  select c;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var whereClause = (WhereClause) queryModel.BodyClauses.Single ();
      Assert.That (whereClause.Predicate, Is.InstanceOf<SubQueryExpression> ());

      var subQueryModel = ((SubQueryExpression) whereClause.Predicate).QueryModel;
      Assert.That (subQueryModel.ResultOperators, Has.Count.EqualTo (1));
      Assert.That (subQueryModel.ResultOperators.Single(), Is.TypeOf<ContainsResultOperator>());
    }

    [Test]
    public void All ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).All (s => s.IsFullTimeCook));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (bool)));

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (AllResultOperator)));

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
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (AnyResultOperator)));
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
      Assert.That (queryModel.BodyClauses[0], Is.InstanceOf (typeof (WhereClause)));

      CheckResolvedExpression<Cook, bool> (((WhereClause) queryModel.BodyClauses[0]).Predicate, queryModel.MainFromClause, s => s.IsFullTimeCook);
      
      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (AnyResultOperator)));
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
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (AggregateResultOperator)));

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
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (AggregateFromSeedResultOperator)));

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
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (AggregateFromSeedResultOperator)));

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

    [Test]
    public void Aggregate_Seed_FuncParameterAssignableFromSeedValue ()
    {
      var expression = ExpressionHelper.MakeExpression (
          () => (from s in QuerySource
                 select s).Aggregate<Cook, IConvertible> ("12", (convertible, s) => convertible.ToInt32 (Thread.CurrentThread.CurrentCulture) + s.ID));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IConvertible)));

      CheckConstantQuerySource (queryModel.MainFromClause.FromExpression, QuerySource);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (AggregateFromSeedResultOperator)));

      var resultOperator = (AggregateFromSeedResultOperator) queryModel.ResultOperators[0];
      Assert.That (resultOperator.Seed.Type, Is.SameAs (typeof (string)));
      Assert.That (resultOperator.Func.Parameters[0].Type, Is.SameAs (typeof (IConvertible)));
    }
  }
}
