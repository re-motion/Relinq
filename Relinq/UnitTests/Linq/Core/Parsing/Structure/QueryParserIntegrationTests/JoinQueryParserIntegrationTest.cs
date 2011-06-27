// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.Core.TestUtilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
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
