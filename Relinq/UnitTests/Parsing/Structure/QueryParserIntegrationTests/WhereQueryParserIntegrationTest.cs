// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.UnitTests.TestDomain;
using Remotion.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
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
