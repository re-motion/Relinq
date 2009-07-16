// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.TestDomain;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.QueryParserIntegrationTests
{
  public abstract class QueryParserIntegrationTestBase
  {
    public IQueryable<Student> QuerySource { get; private set; }
    public QueryParser QueryParser { get; private set; }
    public IQueryable<IndustrialSector> IndustrialSectorQuerySource { get; private set; }
    public IQueryable<Student_Detail> DetailQuerySource { get; private set; }

    [SetUp]
    public void SetUp ()
    {
      QuerySource = ExpressionHelper.CreateQuerySource ();
      IndustrialSectorQuerySource = ExpressionHelper.CreateQuerySource_IndustrialSector();
      DetailQuerySource = ExpressionHelper.CreateQuerySource_Detail ();
      QueryParser = new QueryParser ();
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
      Assert.That (expression, Is.InstanceOfType (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) expression).Value, Is.SameAs (expectedQuerySource));
    }
  }
}