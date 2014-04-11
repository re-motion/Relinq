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
  public class DelegateInvocationQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "Could not parse expression 'c.Assistants.Select(value(System.Func`2[Remotion.Linq.UnitTests.TestDomain.Cook,System.String]))': "
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
    [Ignore ("TODO 4769")]
    public void CompileInvokeCombination ()
    {
      Expression<Func<Cook, bool>> predicate1 = c => c.ID > 100;
      Expression<Func<Cook, bool>> predicate2 = c => c.Name != null;

      var query = QuerySource.Where (c => predicate1.Compile () (c) && predicate2.Compile () (c));

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var predicate = ((WhereClause) queryModel.BodyClauses[0]).Predicate;
      CheckResolvedExpression<Cook, bool> (predicate, queryModel.MainFromClause, c => c.ID > 100 && c.Name != null);
    }
  }
}
