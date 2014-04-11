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
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class DynamicLinqQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void EmptyParameterNames_CanBeParsed ()
    {
      var parameterExpression = Expression.Parameter (typeof (Cook), "");
      var predicate =
          Expression.Equal (
                  Expression.MakeMemberAccess (parameterExpression, typeof (Cook).GetProperty ("Name")),
                  Expression.Constant ("Test"));
      var selector = Expression.MakeMemberAccess (parameterExpression, typeof (Cook).GetProperty ("ID"));
      var query = QuerySource
          .Where (Expression.Lambda<Func<Cook, bool>> (predicate, parameterExpression))
          .Select (Expression.Lambda<Func<Cook, int>> (selector, parameterExpression));

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var whereClause = (WhereClause) queryModel.BodyClauses.Single();
      CheckResolvedExpression<Cook, bool> (whereClause.Predicate, queryModel.MainFromClause, c => c.Name == "Test");

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, int> (selectClause.Selector, queryModel.MainFromClause, c => c.ID);

      Assert.That (queryModel.MainFromClause.ItemName, Is.StringStarting ("<generated>_"));
    }
     
  }
}