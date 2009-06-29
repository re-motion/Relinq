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
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class ClauseMappingTest
  {
    private ClauseMapping _mapping;
    private MainFromClause _clause1;
    private MainFromClause _clause2;
    private MainFromClause _clause3;

    private QuerySourceReferenceExpression _querySourceReferenceExpression1;
    private QuerySourceReferenceExpression _querySourceReferenceExpression2;
    private QuerySourceReferenceExpression _querySourceReferenceExpression3;

    [SetUp]
    public void SetUp ()
    {
      _mapping = new ClauseMapping ();
      _clause1 = ExpressionHelper.CreateMainFromClause ();
      _clause2 = ExpressionHelper.CreateMainFromClause ();
      _clause3 = ExpressionHelper.CreateMainFromClause ();

      _querySourceReferenceExpression1 = new QuerySourceReferenceExpression (_clause1);
      _querySourceReferenceExpression2 = new QuerySourceReferenceExpression (_clause2);
      _querySourceReferenceExpression3 = new QuerySourceReferenceExpression (_clause3);
    }

    [Test]
    public void AddMapping ()
    {
      _mapping.AddMapping (_clause1, _querySourceReferenceExpression1);
      Assert.That (_mapping.GetExpression (_clause1), Is.SameAs (_querySourceReferenceExpression1));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Clause has already been associated with an expression.")]
    public void AddMapping_Twice ()
    {
      _mapping.AddMapping (_clause1, _querySourceReferenceExpression1);
      _mapping.AddMapping (_clause1, _querySourceReferenceExpression2);
    }

    [Test]
    public void ReplaceMapping ()
    {
      _mapping.AddMapping (_clause1, _querySourceReferenceExpression2);
      _mapping.ReplaceMapping (_clause1, _querySourceReferenceExpression3);

      Assert.That (_mapping.GetExpression (_clause1), Is.SameAs (_querySourceReferenceExpression3));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Clause has not been associated with an expression, cannot replace its mapping.")]
    public void ReplaceMapping_WithoutAdding ()
    {
      _mapping.ReplaceMapping (_clause1, _querySourceReferenceExpression2);
    }

    [Test]
    public void ContainsMapping_True ()
    {
      _mapping.AddMapping (_clause1, _querySourceReferenceExpression2);
      Assert.That (_mapping.ContainsMapping (_clause1), Is.True);
    }

    [Test]
    public void ContainsMapping_False ()
    {
      Assert.That (_mapping.ContainsMapping (_clause1), Is.False);
    }

    [Test]
    [ExpectedException (typeof (KeyNotFoundException), ExpectedMessage = "Clause has not been associated with an expression.")]
    public void GetExpression_WithoutAssociatedClause ()
    {
      _mapping.GetExpression (_clause1);
    }

  }
}