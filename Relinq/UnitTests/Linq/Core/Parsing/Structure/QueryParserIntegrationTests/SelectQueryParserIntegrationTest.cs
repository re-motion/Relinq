// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.Core.TestQueryGenerators;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
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
