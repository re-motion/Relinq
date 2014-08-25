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
