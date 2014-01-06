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
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
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
      QuerySource = ExpressionHelper.CreateCookQueryable ();
      IndustrialSectorQuerySource = ExpressionHelper.CreateRestaurantQueryable();
      DetailQuerySource = ExpressionHelper.CreateKitchenQueryable ();
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
  }
}
