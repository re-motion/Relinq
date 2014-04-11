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
