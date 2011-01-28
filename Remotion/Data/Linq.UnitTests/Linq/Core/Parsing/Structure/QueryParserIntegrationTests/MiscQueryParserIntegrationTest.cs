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
using System.Linq.Expressions;
using NUnit.Framework;
using System.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class MiscQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = 
        "Could not parse expression 'c.Assistants.Select(value(System.Func`2[Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain.Cook,System.String]))': "
        + "Object of type 'System.Linq.Expressions.ConstantExpression' cannot be converted to type 'System.Linq.Expressions.LambdaExpression'. "
        + "If you tried to pass a delegate instead of a LambdaExpression, this is not supported because delegates are not parsable expressions.")]
    public void DelegateAsSelector ()
    {
      Func<Cook, string> expression = c => c.FirstName;
      var query = QuerySource.Select (c => c.Assistants.Select (expression));
      QueryParser.GetParsedQuery (query.Expression);
   }

    [Test]
    public void InvocationExpression_AppliedToLambdaExpression ()
    {
      Expression<Func<Cook, bool>> predicate1 = c => c.ID > 100;
      Expression<Func<Cook, bool>> predicate2 = c => c.Name != null;

      // c => c.ID > 100 && ((c1 => c1.Name != null) (c))
      var combinedPredicate =
          Expression.Lambda<Func<Cook, bool>> (
              Expression.AndAlso (
                  predicate1.Body,
                  Expression.Invoke (predicate2, predicate1.Parameters.Cast<Expression> ())
              ),
          predicate1.Parameters);

      var query = QuerySource.Where (combinedPredicate);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var predicate = ((WhereClause) queryModel.BodyClauses[0]).Predicate;
      CheckResolvedExpression<Cook, bool> (predicate, queryModel.MainFromClause, c => c.ID > 100 && c.Name != null);
    }

    [Test]
    public void InvocationExpression_AppliedToInlineLambdaExpression ()
    {
      var query = QuerySource.Where (c => ((Func<Cook, bool>) (c1 => c1.Name != null)) (c)).Select (c => c.FirstName);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var predicate = ((WhereClause) queryModel.BodyClauses[0]).Predicate;
      CheckResolvedExpression<Cook, bool> (predicate, queryModel.MainFromClause, c => c.Name != null);
    }

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