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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.UnitTests.Linq.TestDomain;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class ResultOperatorQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void SubQueryInMainFromClauseWithResultOperator ()
    {
      var query = from s in
                      (from sd1 in DetailQuerySource select sd1.Student).Take (5)
                  from sd in DetailQuerySource
                  select new Tuple<Student, Student_Detail> ( s, sd );
      var expression = query.Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetResultType(), Is.SameAs (typeof (IQueryable<Tuple<Student, Student_Detail>>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo("s"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (TakeResultOperator)));
      Assert.That (subQueryModel.GetResultType(), Is.SameAs (typeof (IQueryable<Student>)));
      
      var selectClause = queryModel.SelectClause;
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
      var query = (from s in ExpressionHelper.CreateStudentQueryable ()
                   select s).Distinct ().Where (x => x.ID > 0);
      
      var expression = query.Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetResultType(), Is.SameAs (typeof (IQueryable<Student>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("x"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (DistinctResultOperator)));
      Assert.That (subQueryModel.GetResultType(), Is.SameAs (typeof (IQueryable<Student>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, mainFromClause, x => x.ID > 0);

      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }

    [Test]
    public void PredicateFollowingResultOperator ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in ExpressionHelper.CreateStudentQueryable ()
                                                               select s).Distinct ().Count(x => x.ID > 0));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetResultType(), Is.SameAs (typeof (int)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("x"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (DistinctResultOperator)));
      Assert.That (subQueryModel.GetResultType(), Is.SameAs (typeof (IQueryable<Student>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, mainFromClause, x => x.ID > 0);

      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));

      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (CountResultOperator)));
    }

    [Test]
    public void TakeWithBackReference ()
    {
      var query =
          from s in QuerySource
          from s1 in s.Friends.Take (s.ID)
          select s1;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetResultType (), Is.SameAs (typeof (IQueryable<Student>)));

      var mainFromClause = queryModel.MainFromClause;
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];

      var subQueryModel = ((SubQueryExpression) additionalFromClause.FromExpression).QueryModel;
      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (TakeResultOperator)));

      var takeResultOperator = (TakeResultOperator) subQueryModel.ResultOperators[0];
      CheckResolvedExpression<Student, int> (takeResultOperator.Count, mainFromClause, s => s.ID);

      Assert.That (subQueryModel.GetResultType (), Is.SameAs (typeof (IQueryable<Student>)));
    }

    [Test]
    public void Average ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                          select s.ID).Average());

      var queryModel = QueryParser.GetParsedQuery (expression);

      Assert.That (queryModel.GetResultType (), Is.SameAs (typeof (double)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (AverageResultOperator)));
      
      CheckResolvedExpression<Student, int> (queryModel.SelectClause.Selector, mainFromClause, s => s.ID);
    }

    [Test]
    public void LongCount ()
    {
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s.ID).LongCount ());

      var queryModel = QueryParser.GetParsedQuery (expression);

      Assert.That (queryModel.GetResultType (), Is.SameAs (typeof (long)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (LongCountResultOperator)));

      CheckResolvedExpression<Student, int> (queryModel.SelectClause.Selector, mainFromClause, s => s.ID);
    }

    [Test]
    public void Skip ()
    {
      var query = (from s in ExpressionHelper.CreateStudentQueryable() 
                   select s.ID).Skip (1);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetResultType (), Is.SameAs (typeof (IQueryable<int>)));

      var mainFromClause = queryModel.MainFromClause;

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (SkipResultOperator)));
      var skipResultOperator = (SkipResultOperator) queryModel.ResultOperators[0];
      CheckResolvedExpression<int,int> (skipResultOperator.Count, mainFromClause, i => 1);
    }

    [Test]
    public void Reverse ()
    {
      var query = (from s in ExpressionHelper.CreateStudentQueryable ()
                   select s).Reverse ();

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetResultType (), Is.SameAs (typeof (IQueryable<Student>)));

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
      IEnumerable<Student> students = new[] { new Student() };
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Except (students));
      
      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetResultType (), Is.SameAs (typeof (IQueryable<Student>)));

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
      IEnumerable<Student> students = new[] { new Student () };
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Intersect (students));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetResultType (), Is.SameAs (typeof (IQueryable<Student>)));

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
      IEnumerable<Student> students = new[] { new Student () };
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Union (students));


      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetResultType (), Is.SameAs (typeof (IQueryable<Student>)));

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
      var student = new Student ();
      var query = (from s in ExpressionHelper.CreateStudentQueryable ()
                   select s).DefaultIfEmpty (student);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetResultType (), Is.SameAs (typeof (IQueryable<Student>)));

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (DefaultIfEmptyResultOperator)));
      Assert.That (((DefaultIfEmptyResultOperator) queryModel.ResultOperators[0]).GetConstantOptionalDefaultValue(), Is.SameAs (student));
    }

    [Test]
    public void Cast ()
    {
      var query = (from s in ExpressionHelper.CreateStudentQueryable()
                   select s.ID).Cast<double>();
      
      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetResultType (), Is.SameAs (typeof (IQueryable<double>)));

      var castResultOperator = (CastResultOperator) queryModel.ResultOperators[0];
      Assert.That (castResultOperator.CastItemType, Is.SameAs (typeof (double)));

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (CastResultOperator)));
    }

    [Test]
    public void Contains ()
    {
      var student = new Student ();
      var expression = ExpressionHelper.MakeExpression (() => (from s in QuerySource
                                                               select s).Contains (student));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetResultType (), Is.SameAs (typeof (bool)));

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOfType (typeof (ContainsResultOperator)));
    }
  }
}