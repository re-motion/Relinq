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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class NullableQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void NullableHasValue_ReplacedByNullCheck ()
    {
      var query = DetailQuerySource.Where (k => k.LastCleaningDay.HasValue);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var predicate = ((WhereClause) queryModel.BodyClauses[0]).Predicate;
      var expectedExpression =
          Expression.NotEqual (
              Expression.MakeMemberAccess (
                  new QuerySourceReferenceExpression (queryModel.MainFromClause), 
                  typeof (Kitchen).GetProperty ("LastCleaningDay")),
              Expression.Constant (null, typeof (DateTime?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, predicate);
    }

    [Test]
    public void NullableValue_ReplacedByCast ()
    {
// ReSharper disable PossibleInvalidOperationException
      var query = DetailQuerySource.Select (k => k.LastCleaningDay.Value);
// ReSharper restore PossibleInvalidOperationException

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var selector = queryModel.SelectClause.Selector;
// ReSharper disable PossibleInvalidOperationException
      CheckResolvedExpression<Kitchen, DateTime> (selector, queryModel.MainFromClause, k => (DateTime) k.LastCleaningDay);
// ReSharper restore PossibleInvalidOperationException
    }
  }
}
