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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class OrderingTest
  {
    [Test]
    public void InitializeWithExpressionAndOrderDirectionAsc()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression();
      const OrderingDirection directionAsc = OrderingDirection.Asc;

      var clause = ExpressionHelper.CreateOrderByClause();
      var ordering = new Ordering(clause, expression,directionAsc);

      Assert.AreSame (clause, ordering.OrderByClause);
      Assert.AreSame (expression, ordering.Expression);
      Assert.AreEqual (directionAsc, ordering.OrderingDirection);
    }

    [Test]
    public void InitializeWithExpressionAndOrderDirectionDesc ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      const OrderingDirection directionAsc = OrderingDirection.Asc;

      var clause = ExpressionHelper.CreateOrderByClause ();
      var ordering = new Ordering (clause,expression, directionAsc);

      Assert.AreSame (clause, ordering.OrderByClause);
      Assert.AreSame (expression, ordering.Expression);
      Assert.AreEqual (directionAsc, ordering.OrderingDirection);
    }

    [Test]
    public void Accept()
    {
      Ordering ordering = ExpressionHelper.CreateOrdering ();

      var repository = new MockRepository();
      var visitorMock = repository.StrictMock<IQueryVisitor>();

      visitorMock.VisitOrdering (ordering);

      repository.ReplayAll();

      ordering.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      Ordering ordering = ExpressionHelper.CreateOrdering ();
      Assert.IsNull (ordering.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      Ordering ordering = ExpressionHelper.CreateOrdering ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      ordering.SetQueryModel (model);
      Assert.IsNotNull (ordering.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      Ordering ordering = ExpressionHelper.CreateOrdering ();
      ordering.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      Ordering ordering = ExpressionHelper.CreateOrdering ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      ordering.SetQueryModel (model);
      ordering.SetQueryModel (model);
    }

    [Test]
    public void Clone ()
    {
      var orderingClause = ExpressionHelper.CreateOrdering ();
      var newOrderByClause = ExpressionHelper.CreateOrderByClause();
      var clone = orderingClause.Clone (newOrderByClause);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (orderingClause));
      Assert.That (clone.Expression, Is.SameAs (orderingClause.Expression));
      Assert.That (clone.OrderingDirection, Is.EqualTo (orderingClause.OrderingDirection));
      Assert.That (clone.QueryModel, Is.Null);
      Assert.That (clone.OrderByClause, Is.SameAs (newOrderByClause));
    }
  }
}
