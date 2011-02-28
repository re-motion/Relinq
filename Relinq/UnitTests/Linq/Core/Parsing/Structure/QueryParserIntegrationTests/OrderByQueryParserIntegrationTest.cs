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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestQueryGenerators;
using Remotion.Linq.Clauses;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class OrderByQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void OrderByAndThenBy ()
    {
      var expression = OrderByTestQueryGenerator.CreateOrderByQueryWithOrderByAndThenBy (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);

      var orderByClause = (OrderByClause) queryModel.BodyClauses[0];
      Assert.That (orderByClause.Orderings.Count, Is.EqualTo (3));

      var ordering1 = orderByClause.Orderings[0];
      Assert.That (ordering1.OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
      CheckResolvedExpression<Cook, string> (ordering1.Expression, mainFromClause, s => s.FirstName);

      var ordering2 = orderByClause.Orderings[1];
      Assert.That (ordering2.OrderingDirection, Is.EqualTo (OrderingDirection.Desc));
      CheckResolvedExpression<Cook, string> (ordering2.Expression, mainFromClause, s => s.Name);

      var ordering3 = orderByClause.Orderings[2];
      Assert.That (ordering3.OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
      CheckResolvedExpression<Cook, List<int>> (ordering3.Expression, mainFromClause, s => s.Holidays);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void MultipleOrderBys ()
    {
      var expression = OrderByTestQueryGenerator.CreateOrderByQueryWithMultipleOrderBys (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      
      var orderByClause1 = (OrderByClause) queryModel.BodyClauses[0];
      Assert.That (orderByClause1.Orderings.Count, Is.EqualTo (3));

      var orderByClause2 = (OrderByClause) queryModel.BodyClauses[1];
      Assert.That (orderByClause2.Orderings.Count, Is.EqualTo (1));
    }

    [Test]
    public void OrderByAndWhere ()
    {
      var expression = MixedTestQueryGenerator.CreateOrderByWithWhereCondition (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.FirstName == "Garcia");

      var orderByClause = (OrderByClause) queryModel.BodyClauses[1];
      CheckResolvedExpression<Cook, string> (orderByClause.Orderings[0].Expression, queryModel.MainFromClause, s1 => s1.FirstName);
    }

    [Test]
    public void MultiFromsWithOrderBy ()
    {
      var expression = MixedTestQueryGenerator.CreateMultiFromWhereOrderByQuery (QuerySource, QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);
      
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      CheckConstantQuerySource (additionalFromClause.FromExpression, QuerySource);

      var whereClause = (WhereClause) queryModel.BodyClauses[1];
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, queryModel.MainFromClause, s1 => s1.Name == "Garcia");

      var orderByClause = (OrderByClause) queryModel.BodyClauses[2];
      Assert.That (orderByClause.Orderings[0].OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
      CheckResolvedExpression<Cook, string> (orderByClause.Orderings[0].Expression, queryModel.MainFromClause, s1 => s1.FirstName);
      Assert.That (orderByClause.Orderings[1].OrderingDirection, Is.EqualTo (OrderingDirection.Desc));
      CheckResolvedExpression<Cook, string> (orderByClause.Orderings[1].Expression, additionalFromClause, s2 => s2.Name);
    }
  }
}
