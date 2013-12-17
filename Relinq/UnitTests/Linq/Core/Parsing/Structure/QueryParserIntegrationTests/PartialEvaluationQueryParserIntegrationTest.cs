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
using System.Linq.Expressions;
using NUnit.Framework;
using System.Linq;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
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

      CheckConstantQuerySource (subQuery.MainFromClause.FromExpression, QuerySource);
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
