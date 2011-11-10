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
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.Core.TestQueryGenerators;
using Remotion.Linq.Clauses;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class WhereQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void SimpleWhere ()
    {
      var expression = WhereTestQueryGenerator.CreateSimpleWhereQuery (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      Assert.That (queryModel.MainFromClause.ItemName, Is.EqualTo ("s"));
      
      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, queryModel.MainFromClause, s => s.Name == "Garcia");

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void ThreeWheres ()
    {
      var expression = WhereTestQueryGenerator.CreateMultiWhereQuery (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (3));

      var whereClause1 = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, bool> (whereClause1.Predicate, queryModel.MainFromClause, s => s.Name == "Garcia");

      var whereClause2 = (WhereClause) queryModel.BodyClauses[1];
      CheckResolvedExpression<Cook, bool> (whereClause2.Predicate, queryModel.MainFromClause, s => s.FirstName == "Hugo");

      var whereClause3 = (WhereClause) queryModel.BodyClauses[2];
      CheckResolvedExpression<Cook, bool> (whereClause3.Predicate, queryModel.MainFromClause, s => s.ID > 100);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void WhereWithDifferentComparisons ()
    {
      var expression = WhereTestQueryGenerator.CreateWhereQueryWithDifferentComparisons (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      CheckConstantQuerySource (queryModel.MainFromClause.FromExpression, QuerySource);

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, 
                                              queryModel.MainFromClause, s => s.FirstName != "Garcia" && s.ID > 5 && s.ID >= 6 && s.ID < 7 && s.ID <= 6 && s.ID == 6);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }
  }
}
