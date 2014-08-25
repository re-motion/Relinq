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
using NUnit.Framework;
using Remotion.Linq.UnitTests.TestDomain;
using Remotion.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class SelectQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void SimpleSelect ()
    {
      var expression = SelectTestQueryGenerator.CreateSimpleQuery (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, queryModel.MainFromClause, s => s);
    }

    [Test]
    public void Let ()
    {
      var expression = LetTestQueryGenerator.CreateSimpleLetClause (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      var selectClause = queryModel.SelectClause;

      Assert.That (queryModel.BodyClauses.Count (), Is.EqualTo (0));
      CheckResolvedExpression<Cook, string> (selectClause.Selector, mainFromClause, s => s.FirstName + s.Name);
    }
  }
}
