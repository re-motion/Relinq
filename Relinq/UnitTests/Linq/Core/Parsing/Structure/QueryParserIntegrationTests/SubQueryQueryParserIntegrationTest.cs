// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.Core.TestQueryGenerators;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class SubQueryQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void SimpleSubQuery_InAdditionalFromClause ()
    {
      var expression = SubQueryTestQueryGenerator.CreateSimpleSubQueryInAdditionalFromClause (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (1));
      var subQueryFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];

      var subQueryModel = ((SubQueryExpression) subQueryFromClause.FromExpression).QueryModel;
      var subQueryMainFromClause = subQueryModel.MainFromClause;
      Assert.That (subQueryMainFromClause.ItemName, Is.EqualTo ("s3"));
      CheckConstantQuerySource (subQueryMainFromClause.FromExpression, QuerySource);

      var subQuerySelectClause = subQueryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (subQuerySelectClause.Selector, subQueryMainFromClause, s3 => s3);
    }

    [Test]
    public void NestedSubQuery_InAdditionalFromClause ()
    {
      var queryable = from s in QuerySource from s2 in (from s3 in QuerySource from s4 in (from s5 in QuerySource select s5) select s3) select s;
      var queryModel = QueryParser.GetParsedQuery (queryable.Expression);

      var outerSubQueryFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      Assert.That (outerSubQueryFromClause.FromExpression, Is.TypeOf (typeof (SubQueryExpression)));

      var outerSubQuery = ((SubQueryExpression) outerSubQueryFromClause.FromExpression).QueryModel;
      var innerSubQueryFromClause = (AdditionalFromClause) outerSubQuery.BodyClauses[0];
      Assert.That (innerSubQueryFromClause.FromExpression, Is.TypeOf (typeof (SubQueryExpression)));
    }

    [Test]
    public void SubQuery_InNewExpression_RetainsType ()
    {
      var queryable = from s in QuerySource select new { Result = from s2 in (IEnumerable<Cook>) QuerySource select s2 };
      var queryModel = QueryParser.GetParsedQuery (queryable.Expression);

      Assert.That (queryModel.SelectClause.Selector, Is.TypeOf (typeof (NewExpression)));
      Assert.That (((NewExpression) queryModel.SelectClause.Selector).Arguments[0], Is.TypeOf (typeof (SubQueryExpression)));
      Assert.That (((NewExpression) queryModel.SelectClause.Selector).Arguments[0].Type, Is.SameAs (typeof (IEnumerable<Cook>)));

      var subQueryModel = ((SubQueryExpression) ((NewExpression) queryModel.SelectClause.Selector).Arguments[0]).QueryModel;
      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IEnumerable<Cook>)));
    }

    [Test]
    public void SubQuery_InMainExpressionNode ()
    {
      var queryExpression = ExpressionHelper.MakeExpression (() => new { Result = (from k in QuerySource select k) }.Result.Count ());
      var queryModel = QueryParser.GetParsedQuery (queryExpression);

      Assert.That (queryModel.IsIdentityQuery (), Is.True);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.TypeOf (typeof (CountResultOperator)));

      var fromExpression = (MemberExpression) queryModel.MainFromClause.FromExpression;
      var newExpression = (NewExpression) fromExpression.Expression;
      
      var innerSubQuery = (SubQueryExpression) newExpression.Arguments[0];
      Assert.That (innerSubQuery.QueryModel.IsIdentityQuery (), Is.True);
      CheckConstantQuerySource (innerSubQuery.QueryModel.MainFromClause.FromExpression, QuerySource);
    }

    [Test]
    public void SubQuery_InAggregateResultOperatpr ()
    {
      var queryExpression = ExpressionHelper.MakeExpression (() => (
          from c in QuerySource 
          select c).Aggregate (0, (sum, current) => sum + current.Assistants.Count()));
      var queryModel = QueryParser.GetParsedQuery (queryExpression);

      Assert.That (queryModel.IsIdentityQuery (), Is.True);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.TypeOf (typeof (AggregateFromSeedResultOperator)));

      var aggregatorFunc = ((AggregateFromSeedResultOperator) queryModel.ResultOperators[0]).Func;
      Assert.That (aggregatorFunc.Body, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (((BinaryExpression) aggregatorFunc.Body).Left, Is.SameAs (aggregatorFunc.Parameters[0]));
      Assert.That (((BinaryExpression) aggregatorFunc.Body).Right, Is.TypeOf (typeof (SubQueryExpression)));

      var subQuery = ((SubQueryExpression) ((BinaryExpression) aggregatorFunc.Body).Right).QueryModel;
      Assert.That (subQuery.IsIdentityQuery (), Is.True);
      CheckResolvedExpression<Cook, IQueryable<Cook>> (
          subQuery.MainFromClause.FromExpression, 
          queryModel.MainFromClause, 
          current => current.Assistants);

      Assert.That (subQuery.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQuery.ResultOperators[0], Is.TypeOf (typeof (CountResultOperator)));
    }
  }
}
