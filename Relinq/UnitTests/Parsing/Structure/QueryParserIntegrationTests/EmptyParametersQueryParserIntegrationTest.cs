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