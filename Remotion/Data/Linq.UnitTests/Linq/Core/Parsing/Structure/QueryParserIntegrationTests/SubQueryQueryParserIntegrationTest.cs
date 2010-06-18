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
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class SubQueryQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void SimpleSubQuery_InAdditionalFromClause ()
    {
      var expression = SubQueryTestQueryGenerator.CreateSimpleSubQueryInAdditionalFromClause (QuerySource).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (1));
      var subQueryFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];

      var subQueryModel = ((SubQueryExpression) subQueryFromClause.FromExpression).QueryModel;
      var subQueryMainFromClause = subQueryModel.MainFromClause;
      Assert.That (subQueryMainFromClause.ItemName, Is.EqualTo ("s3"));
      CheckConstantQuerySource (subQueryMainFromClause.FromExpression, QuerySource);

      var subQuerySelectClause = subQueryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (subQuerySelectClause.Selector, subQueryMainFromClause, s3 => s3);
    }

    [Test]
    public void NestedSubQuery_InAdditionalFromClause ()
    {
      var queryable = from s in QuerySource from s2 in (from s3 in QuerySource from s4 in (from s5 in QuerySource select s5) select s3) select s;
      var queryModel = QueryParser.GetParsedQuery (queryable.Expression);

      var outerSubQueryFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      Assert.That (outerSubQueryFromClause.FromExpression, Is.TypeOf (typeof (SubQueryExpression)));

      var outerSubQuery = ((SubQueryExpression) outerSubQueryFromClause.FromExpression).QueryModel;
      var innerSubQueryFromClause = (AdditionalFromClause) outerSubQuery.BodyClauses[0];
      Assert.That (innerSubQueryFromClause.FromExpression, Is.TypeOf (typeof (SubQueryExpression)));
    }
  }
}
