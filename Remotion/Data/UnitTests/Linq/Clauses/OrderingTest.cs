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
    private ClonedClauseMapping _clonedClauseMapping;
    private OrderByClause _orderByClause;
    private Ordering _ordering;

    [SetUp]
    public void SetUp ()
    {
      _clonedClauseMapping = new ClonedClauseMapping ();
      _orderByClause = ExpressionHelper.CreateOrderByClause ();
      _ordering = ExpressionHelper.CreateOrdering (_orderByClause);
    }

    [Test]
    public void InitializeWithExpressionAndOrderDirectionAsc()
    {
      var expression = ExpressionHelper.CreateExpression ();

      var ordering = new Ordering(_orderByClause, expression, OrderingDirection.Asc);

      Assert.That (ordering.OrderByClause, Is.SameAs (_orderByClause));
      Assert.That (ordering.Expression, Is.SameAs (expression));
      Assert.That (ordering.OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
    }

    [Test]
    public void InitializeWithExpressionAndOrderDirectionDesc ()
    {
      var expression = ExpressionHelper.CreateExpression ();
      const OrderingDirection directionAsc = OrderingDirection.Asc;

      var ordering = new Ordering (_orderByClause, expression, directionAsc);

      Assert.That (ordering.OrderByClause, Is.SameAs (_orderByClause));
      Assert.That (ordering.Expression, Is.SameAs (expression));
      Assert.That (ordering.OrderingDirection, Is.EqualTo (directionAsc));
    }

    [Test]
    public void Accept()
    {
      var repository = new MockRepository();
      var visitorMock = repository.StrictMock<IQueryVisitor>();

      visitorMock.VisitOrdering (_ordering);

      repository.ReplayAll();

      _ordering.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      Assert.That (_ordering.QueryModel, Is.Null);
    }

    [Test]
    public void SetQueryModel ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _ordering.SetQueryModel (model);
      Assert.That (_ordering.QueryModel, Is.Not.Null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      _ordering.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _ordering.SetQueryModel (model);
      _ordering.SetQueryModel (model);
    }

    [Test]
    public void Clone ()
    {
      var newOrderByClause = ExpressionHelper.CreateOrderByClause();
      _clonedClauseMapping.AddMapping (_orderByClause, newOrderByClause);
      var clone = _ordering.Clone (_clonedClauseMapping);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_ordering));
      Assert.That (clone.Expression, Is.SameAs (_ordering.Expression));
      Assert.That (clone.OrderingDirection, Is.EqualTo (_ordering.OrderingDirection));
      Assert.That (clone.QueryModel, Is.Null);
      Assert.That (clone.OrderByClause, Is.SameAs (newOrderByClause));
    }
  }
}
