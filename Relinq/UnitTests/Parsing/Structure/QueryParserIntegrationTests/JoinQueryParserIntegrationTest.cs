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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class JoinQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void Join ()
    {
      var query = from s in QuerySource
                  join sd in DetailQuerySource on s.ID equals sd.RoomNumber
                  select Tuple.Create (s, sd);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Tuple<Cook, Kitchen>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var joinClause = ((JoinClause) queryModel.BodyClauses[0]);
      CheckConstantQuerySource (joinClause.InnerSequence, DetailQuerySource);
      Assert.That (joinClause.ItemType, Is.SameAs (typeof (Kitchen)));
      Assert.That (joinClause.ItemName, Is.EqualTo ("sd"));
      CheckResolvedExpression<Cook, int> (joinClause.OuterKeySelector, mainFromClause, s => s.ID);
      CheckResolvedExpression<Kitchen, int> (joinClause.InnerKeySelector, joinClause, sd => sd.RoomNumber);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Kitchen, Tuple<Cook, Kitchen>> (
          selectClause.Selector, 
          mainFromClause, 
          joinClause, 
          (s, sd) => Tuple.Create (s, sd));
    }

    [Test]
    public void Join_InnerSequenceDependingOnOuter ()
    {
      var query = from s in QuerySource
                  from s2 in (from s1 in QuerySource join s2 in s.Assistants on s.ID equals s2.ID select s2)
                  select s2;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      var mainFromClause = queryModel.MainFromClause;
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      
      var innerJoinClause = ((JoinClause) ((SubQueryExpression) additionalFromClause.FromExpression).QueryModel.BodyClauses[0]);
      CheckResolvedExpression<Cook, IEnumerable<Cook>> (innerJoinClause.InnerSequence, mainFromClause, s => s.Assistants);
      Assert.That (innerJoinClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (innerJoinClause.ItemName, Is.EqualTo ("s2"));
    }

    [Test]
    public void Join_WithoutSelect ()
    {
      var query = QuerySource.Join (DetailQuerySource, s => s, sd => sd.Cook, (s, sd) => Tuple.Create (s, sd));

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Tuple<Cook, Kitchen>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var joinClause = (JoinClause) queryModel.BodyClauses[0];
      CheckConstantQuerySource (joinClause.InnerSequence, DetailQuerySource);
      Assert.That (joinClause.ItemType, Is.SameAs (typeof (Kitchen)));
      Assert.That (joinClause.ItemName, Is.EqualTo ("sd"));
      CheckResolvedExpression<Cook, Cook> (joinClause.OuterKeySelector, mainFromClause, s => s);
      CheckResolvedExpression<Kitchen, Cook> (joinClause.InnerKeySelector, joinClause, sd => sd.Cook);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Kitchen, Tuple<Cook, Kitchen>> (
          selectClause.Selector,
          mainFromClause,
          joinClause,
          (s, sd) => Tuple.Create (s, sd));
    }
  }
}
