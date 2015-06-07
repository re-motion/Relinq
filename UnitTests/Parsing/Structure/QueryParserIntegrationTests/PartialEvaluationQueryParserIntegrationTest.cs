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
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class PartialEvaluationQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void ConstantReferenceToOtherQuery_IsInlined_AndPartiallyEvaluated ()
    {
      var query1 = from c in QuerySource where 1.ToString () == "1" select c;
      var query2 = from k in DetailQuerySource where query1.Contains (k.Cook) select k;

      // Handle this as if someone had written: from k in DetailQuerySource where (from c in QuerySource select c).Contains (k.Cook) select k;

      var queryModel = QueryParser.GetParsedQuery (query2.Expression);

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      Assert.That (whereClause.Predicate, Is.TypeOf (typeof (SubQueryExpression)));

      var subQuery = ((SubQueryExpression) whereClause.Predicate).QueryModel;

      CheckPartiallyEvaluatedQuerySource (subQuery.MainFromClause.FromExpression, QuerySource);
      CheckResolvedExpression<Cook, Cook> (subQuery.SelectClause.Selector, subQuery.MainFromClause, c => c);

      var subQueryWhereClause = (WhereClause) subQuery.BodyClauses[0];
      var expectedSubQueryWherePredicate = Expression.Constant (true);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSubQueryWherePredicate, subQueryWhereClause.Predicate);

      Assert.That (subQuery.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (subQuery.ResultOperators[0], Is.TypeOf (typeof (ContainsResultOperator)));

      CheckResolvedExpression<Kitchen, Cook> (((ContainsResultOperator) subQuery.ResultOperators[0]).Item, queryModel.MainFromClause, k => k.Cook);
    }

    [Test]
    public void Exception_InEvaluableSubExpression ()
    {
      string nullValue = null;
      var query = from c in QuerySource where nullValue != null && c.ID > nullValue.Length select c.Name;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var whereClause = (WhereClause) queryModel.BodyClauses[0];

      // Expected: false && c.ID > Exception (nullValue.Length)
      Assert.That (whereClause.Predicate, Is.AssignableTo<BinaryExpression>().With.Property ("NodeType").EqualTo (ExpressionType.AndAlso));
      var outerBinary = ((BinaryExpression) whereClause.Predicate);
      CheckResolvedExpression<Cook, bool> (outerBinary.Left, queryModel.MainFromClause, c => false);
      Assert.That (outerBinary.Right, Is.AssignableTo<BinaryExpression> ().With.Property ("NodeType").EqualTo (ExpressionType.GreaterThan));
      var innerBinary = (BinaryExpression) outerBinary.Right;
      CheckResolvedExpression<Cook, int> (innerBinary.Left, queryModel.MainFromClause, c => c.ID);
      Assert.That (
          innerBinary.Right, 
          Is.TypeOf<PartialEvaluationExceptionExpression>().With.Property ("Exception").InstanceOf<NullReferenceException>());
      CheckResolvedExpression<Cook, int> (
          ((PartialEvaluationExceptionExpression) innerBinary.Right).EvaluatedExpression, queryModel.MainFromClause, c => ((string) null).Length);
    }
  }
}
