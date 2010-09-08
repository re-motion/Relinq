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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class VBStringComparisonQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void VBStringComparison_WithParameterInsideComparison ()
    {
      var predicateParameter = Expression.Parameter (typeof (Cook), "c");
      var predicate = Expression.Lambda<Func<Cook, bool>> (
          new VBStringComparisonExpression (Expression.Equal (predicateParameter, Expression.Constant (null)), true), predicateParameter);

      var expression = (from s in QuerySource select s).Where (predicate).Expression;
      var queryModel = QueryParser.GetParsedQuery (expression);

      var expectedExpression = new VBStringComparisonExpression (
          Expression.Equal (new QuerySourceReferenceExpression (queryModel.MainFromClause), 
          Expression.Constant (null)), true);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, ((WhereClause) queryModel.BodyClauses[0]).Predicate);
    }
  }
}
