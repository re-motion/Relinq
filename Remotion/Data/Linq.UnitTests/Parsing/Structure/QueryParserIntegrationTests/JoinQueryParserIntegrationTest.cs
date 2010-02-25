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
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.UnitTests.TestUtilities;

namespace Remotion.Data.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class JoinQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void Join ()
    {
      var query = from s in QuerySource
                  join sd in DetailQuerySource on s.ID equals sd.StudentID
                  select Tuple.Create (s, sd);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Tuple<Chef, Student_Detail>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Chef)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var joinClause = ((JoinClause) queryModel.BodyClauses[0]);
      CheckConstantQuerySource (joinClause.InnerSequence, DetailQuerySource);
      Assert.That (joinClause.ItemType, Is.SameAs (typeof (Student_Detail)));
      Assert.That (joinClause.ItemName, Is.EqualTo ("sd"));
      CheckResolvedExpression<Chef, int> (joinClause.OuterKeySelector, mainFromClause, s => s.ID);
      CheckResolvedExpression<Student_Detail, int> (joinClause.InnerKeySelector, joinClause, sd => sd.StudentID);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Chef, Student_Detail, Tuple<Chef, Student_Detail>> (
          selectClause.Selector, 
          mainFromClause, 
          joinClause, 
          (s, sd) => Tuple.Create (s, sd));
    }

    [Test]
    public void Join_InnerSequenceDependingOnOuter ()
    {
      var query = from s in QuerySource
                  from s2 in (from s1 in QuerySource join s2 in s.Friends on s.ID equals s2.ID select s2)
                  select s2;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      var mainFromClause = queryModel.MainFromClause;
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      
      var innerJoinClause = ((JoinClause) ((SubQueryExpression) additionalFromClause.FromExpression).QueryModel.BodyClauses[0]);
      CheckResolvedExpression<Chef, IEnumerable<Chef>> (innerJoinClause.InnerSequence, mainFromClause, s => s.Friends);
      Assert.That (innerJoinClause.ItemType, Is.SameAs (typeof (Chef)));
      Assert.That (innerJoinClause.ItemName, Is.EqualTo ("s2"));
    }

    [Test]
    public void Join_WithoutSelect ()
    {
      var query = QuerySource.Join (DetailQuerySource, s => s, sd => sd.Chef, (s, sd) => Tuple.Create (s, sd));

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Tuple<Chef, Student_Detail>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Chef)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var joinClause = (JoinClause) queryModel.BodyClauses[0];
      CheckConstantQuerySource (joinClause.InnerSequence, DetailQuerySource);
      Assert.That (joinClause.ItemType, Is.SameAs (typeof (Student_Detail)));
      Assert.That (joinClause.ItemName, Is.EqualTo ("sd"));
      CheckResolvedExpression<Chef, Chef> (joinClause.OuterKeySelector, mainFromClause, s => s);
      CheckResolvedExpression<Student_Detail, Chef> (joinClause.InnerKeySelector, joinClause, sd => sd.Chef);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Chef, Student_Detail, Tuple<Chef, Student_Detail>> (
          selectClause.Selector,
          mainFromClause,
          joinClause,
          (s, sd) => Tuple.Create (s, sd));
    }
  }
}
