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
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class GroupJoinQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void GroupJoin ()
    {
      var query = from s in QuerySource
                  join sd in DetailQuerySource on s.ID equals sd.RoomNumber into sds
                  select Tuple.Create (s, sds);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Tuple<Cook, IEnumerable<Kitchen>>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var groupJoinClause = ((GroupJoinClause) queryModel.BodyClauses[0]);
      Assert.That (groupJoinClause.ItemName, Is.SameAs ("sds"));
      Assert.That (groupJoinClause.ItemType, Is.SameAs (typeof (IEnumerable<Kitchen>)));
      CheckConstantQuerySource (groupJoinClause.JoinClause.InnerSequence, DetailQuerySource);
      Assert.That (groupJoinClause.JoinClause.ItemType, Is.SameAs (typeof (Kitchen)));
      Assert.That (groupJoinClause.JoinClause.ItemName, Is.EqualTo ("sd"));
      CheckResolvedExpression<Cook, int> (groupJoinClause.JoinClause.OuterKeySelector, mainFromClause, s => s.ID);
      CheckResolvedExpression<Kitchen, int> (groupJoinClause.JoinClause.InnerKeySelector, groupJoinClause.JoinClause, sd => sd.RoomNumber);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, IEnumerable<Kitchen>, Tuple<Cook, IEnumerable<Kitchen>>> (
          selectClause.Selector,
          mainFromClause,
          groupJoinClause,
          (s, sds) => Tuple.Create (s, sds));
    }

    [Test]
    public void GroupJoin_WithoutSelect ()
    {
      var query = QuerySource.GroupJoin (DetailQuerySource, s => s.ID, sd => sd.RoomNumber, (s, sds) => Tuple.Create (s, sds));

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Tuple<Cook, IEnumerable<Kitchen>>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var groupJoinClause = ((GroupJoinClause) queryModel.BodyClauses[0]);
      Assert.That (groupJoinClause.ItemName, Is.SameAs ("sds"));
      Assert.That (groupJoinClause.ItemType, Is.SameAs (typeof (IEnumerable<Kitchen>)));
      CheckConstantQuerySource (groupJoinClause.JoinClause.InnerSequence, DetailQuerySource);
      Assert.That (groupJoinClause.JoinClause.ItemType, Is.SameAs (typeof (Kitchen)));
      Assert.That (groupJoinClause.JoinClause.ItemName, Is.EqualTo ("sd"));
      CheckResolvedExpression<Cook, int> (groupJoinClause.JoinClause.OuterKeySelector, mainFromClause, s => s.ID);
      CheckResolvedExpression<Kitchen, int> (groupJoinClause.JoinClause.InnerKeySelector, groupJoinClause.JoinClause, sd => sd.RoomNumber);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, IEnumerable<Kitchen>, Tuple<Cook, IEnumerable<Kitchen>>> (
          selectClause.Selector,
          mainFromClause,
          groupJoinClause,
          (s, sds) => Tuple.Create (s, sds));
    }
  }
}
