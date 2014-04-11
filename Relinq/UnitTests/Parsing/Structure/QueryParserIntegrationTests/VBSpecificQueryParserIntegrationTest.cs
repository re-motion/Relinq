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
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class VBSpecificQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void VBStringComparison ()
    {
      var parameterExpression = Expression.Parameter (typeof (Cook), "c");
      var vbCompareStringExpression =
          Expression.Equal (
              Expression.Call (
                  typeof (Operators).GetMethod ("CompareString"),
                  Expression.Constant ("string1"),
                  Expression.MakeMemberAccess (parameterExpression, typeof (Cook).GetProperty ("Name")),
                  Expression.Constant (true)),
              Expression.Constant (0));
      var query = QuerySource
          .Where (Expression.Lambda<Func<Cook, bool>> (vbCompareStringExpression, parameterExpression))
          .Select (c => c.Name);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var whereClause = (WhereClause) queryModel.BodyClauses[0];

      var expectedExpression = new VBStringComparisonExpression (
          Expression.Equal (
              Expression.Constant ("string1"),
              Expression.MakeMemberAccess (new QuerySourceReferenceExpression (queryModel.MainFromClause), typeof (Cook).GetProperty ("Name"))),
          true);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, whereClause.Predicate);
    }

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

    [Test]
    public void VBIsNothing ()
    {
      var parameterExpression = Expression.Parameter (typeof (Cook), "c");
      var vbIsNothingExpression = Expression.Call (typeof (Information).GetMethod ("IsNothing"), parameterExpression);
              
      var query = QuerySource
          .Where (Expression.Lambda<Func<Cook, bool>> (vbIsNothingExpression, parameterExpression))
          .Select (c => c.Name);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var whereClause = (WhereClause) queryModel.BodyClauses[0];

      var expectedExpression = Expression.Equal (new QuerySourceReferenceExpression (queryModel.MainFromClause), Expression.Constant (null));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, whereClause.Predicate);
    }
  }
}
