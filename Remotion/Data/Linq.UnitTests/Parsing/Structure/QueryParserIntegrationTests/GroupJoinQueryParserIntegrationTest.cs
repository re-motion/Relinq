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
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class GroupJoinQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void GroupJoin ()
    {
      var query = from s in QuerySource
                  join sd in DetailQuerySource on s.ID equals sd.StudentID into sds
                  select Tuple.NewTuple (s, sds);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Tuple<Student, IEnumerable<Student_Detail>>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var groupJoinClause = ((GroupJoinClause) queryModel.BodyClauses[0]);
      Assert.That (groupJoinClause.ItemName, Is.SameAs ("sds"));
      Assert.That (groupJoinClause.ItemType, Is.SameAs (typeof (IEnumerable<Student_Detail>)));
      CheckConstantQuerySource (groupJoinClause.JoinClause.InnerSequence, DetailQuerySource);
      Assert.That (groupJoinClause.JoinClause.ItemType, Is.SameAs (typeof (Student_Detail)));
      Assert.That (groupJoinClause.JoinClause.ItemName, Is.EqualTo ("sd"));
      CheckResolvedExpression<Student, int> (groupJoinClause.JoinClause.OuterKeySelector, mainFromClause, s => s.ID);
      CheckResolvedExpression<Student_Detail, int> (groupJoinClause.JoinClause.InnerKeySelector, groupJoinClause.JoinClause, sd => sd.StudentID);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Student, IEnumerable<Student_Detail>, Tuple<Student, IEnumerable<Student_Detail>>> (
          selectClause.Selector,
          mainFromClause,
          groupJoinClause,
          (s, sds) => Tuple.NewTuple (s, sds));
    }

    [Test]
    public void GroupJoin_WithoutSelect ()
    {
      var query = QuerySource.GroupJoin (DetailQuerySource, s => s.ID, sd => sd.StudentID, (s, sds) => Tuple.NewTuple (s, sds));

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<Tuple<Student, IEnumerable<Student_Detail>>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var groupJoinClause = ((GroupJoinClause) queryModel.BodyClauses[0]);
      Assert.That (groupJoinClause.ItemName, Is.SameAs ("sds"));
      Assert.That (groupJoinClause.ItemType, Is.SameAs (typeof (IEnumerable<Student_Detail>)));
      CheckConstantQuerySource (groupJoinClause.JoinClause.InnerSequence, DetailQuerySource);
      Assert.That (groupJoinClause.JoinClause.ItemType, Is.SameAs (typeof (Student_Detail)));
      Assert.That (groupJoinClause.JoinClause.ItemName, Is.EqualTo ("sd"));
      CheckResolvedExpression<Student, int> (groupJoinClause.JoinClause.OuterKeySelector, mainFromClause, s => s.ID);
      CheckResolvedExpression<Student_Detail, int> (groupJoinClause.JoinClause.InnerKeySelector, groupJoinClause.JoinClause, sd => sd.StudentID);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Student, IEnumerable<Student_Detail>, Tuple<Student, IEnumerable<Student_Detail>>> (
          selectClause.Selector,
          mainFromClause,
          groupJoinClause,
          (s, sds) => Tuple.NewTuple (s, sds));
    }
  }
}
