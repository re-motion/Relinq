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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  public abstract class QueryParserIntegrationTestBase
  {
    public IQueryable<Cook> QuerySource { get; private set; }
    public QueryParser QueryParser { get; private set; }
    public IQueryable<Restaurant> IndustrialSectorQuerySource { get; private set; }
    public IQueryable<Kitchen> DetailQuerySource { get; private set; }

    [SetUp]
    public void SetUp ()
    {
      QuerySource = ExpressionHelper.CreateQueryable<Cook> ();
      IndustrialSectorQuerySource = ExpressionHelper.CreateQueryable<Restaurant>();
      DetailQuerySource = ExpressionHelper.CreateQueryable<Kitchen> ();
      QueryParser = QueryParser.CreateDefault();
    }

    protected void CheckResolvedExpression<TParameter, TResult> (Expression expressionToCheck, IQuerySource clauseToReference, Expression<Func<TParameter, TResult>> expectedUnresolvedExpression)
    {
      var expectedPredicate = ExpressionHelper.Resolve (clauseToReference, expectedUnresolvedExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedPredicate, expressionToCheck);
    }

    protected void CheckResolvedExpression<TParameter1, TParameter2, TResult> (Expression expressionToCheck, IQuerySource clauseToReference1, IQuerySource clauseToReference2, Expression<Func<TParameter1, TParameter2, TResult>> expectedUnresolvedExpression)
    {
      var expectedPredicate = ExpressionHelper.Resolve (clauseToReference1, clauseToReference2, expectedUnresolvedExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedPredicate, expressionToCheck);
    }

    protected void CheckConstantQuerySource (Expression expression, object expectedQuerySource)
    {
      Assert.That (expression, Is.InstanceOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) expression).Value, Is.SameAs (expectedQuerySource));
    }

    protected void CheckPartiallyEvaluatedQuerySource (Expression expression, object expectedQuerySource)
    {
      Assert.That (expression, Is.InstanceOf (typeof (PartiallyEvaluatedExpression)));
      Assert.That (((PartiallyEvaluatedExpression) expression).EvaluatedExpression.Value, Is.SameAs (expectedQuerySource));
    }
  }
}
