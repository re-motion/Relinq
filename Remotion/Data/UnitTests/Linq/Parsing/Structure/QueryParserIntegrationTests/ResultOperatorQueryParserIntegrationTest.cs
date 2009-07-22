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
  }
}