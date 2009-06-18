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
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.Expressions;
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
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _clonedClauseMapping = new ClonedClauseMapping ();
      _orderByClause = ExpressionHelper.CreateOrderByClause ();
      _ordering = ExpressionHelper.CreateOrdering (_orderByClause);
      _cloneContext = new CloneContext (_clonedClauseMapping, new List<QueryModel> ());
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
      var clone = _ordering.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_ordering));
      Assert.That (clone.Expression, Is.SameAs (_ordering.Expression));
      Assert.That (clone.OrderingDirection, Is.EqualTo (_ordering.OrderingDirection));
      Assert.That (clone.QueryModel, Is.Null);
      Assert.That (clone.OrderByClause, Is.SameAs (newOrderByClause));
    }

    [Test]
    public void Clone_AdjustsExpressions ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause();
      var expression = new QuerySourceReferenceExpression (mainFromClause);
      var whereClause = new Ordering (_orderByClause, expression, OrderingDirection.Asc);

      var newMainFromClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (mainFromClause, newMainFromClause);
      _cloneContext.ClonedClauseMapping.AddMapping (_orderByClause, ExpressionHelper.CreateOrderByClause());

      var clone = whereClause.Clone (_cloneContext);

      Assert.That (((QuerySourceReferenceExpression) clone.Expression).ReferencedClause, Is.SameAs (newMainFromClause));
    }
  }
}
