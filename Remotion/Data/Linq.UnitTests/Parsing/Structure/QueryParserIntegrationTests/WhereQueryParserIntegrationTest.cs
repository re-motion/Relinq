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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
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
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, queryModel.MainFromClause, s => s.Last == "Garcia");

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void ThreeWheres ()
    {
      var expression = WhereTestQueryGenerator.CreateMultiWhereQuery (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (3));

      var whereClause1 = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause1.Predicate, queryModel.MainFromClause, s => s.Last == "Garcia");

      var whereClause2 = (WhereClause) queryModel.BodyClauses[1];
      CheckResolvedExpression<Student, bool> (whereClause2.Predicate, queryModel.MainFromClause, s => s.First == "Hugo");

      var whereClause3 = (WhereClause) queryModel.BodyClauses[2];
      CheckResolvedExpression<Student, bool> (whereClause3.Predicate, queryModel.MainFromClause, s => s.ID > 100);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void WhereWithDifferentComparisons ()
    {
      var expression = WhereTestQueryGenerator.CreateWhereQueryWithDifferentComparisons (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      CheckConstantQuerySource (queryModel.MainFromClause.FromExpression, QuerySource);

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<Student, bool> (whereClause.Predicate, 
                                              queryModel.MainFromClause, s => s.First != "Garcia" && s.ID > 5 && s.ID >= 6 && s.ID < 7 && s.ID <= 6 && s.ID == 6);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Student, Student> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }
  }
}
