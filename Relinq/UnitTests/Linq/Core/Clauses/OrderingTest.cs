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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Clauses
{
  [TestFixture]
  public class OrderingTest
  {
    private QuerySourceMapping _querySourceMapping;
    private Ordering _ordering;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _querySourceMapping = new QuerySourceMapping ();
      _ordering = ExpressionHelper.CreateOrdering ();
      _cloneContext = new CloneContext (_querySourceMapping);
    }

    [Test]
    public void InitializeWithExpressionAndOrderDirectionAsc()
    {
      var expression = ExpressionHelper.CreateExpression ();

      var ordering = new Ordering (expression, OrderingDirection.Asc);

      Assert.That (ordering.Expression, Is.SameAs (expression));
      Assert.That (ordering.OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
    }

    [Test]
    public void InitializeWithExpressionAndOrderDirectionDesc ()
    {
      var expression = ExpressionHelper.CreateExpression ();
      const OrderingDirection directionAsc = OrderingDirection.Asc;

      var ordering = new Ordering (expression, directionAsc);

      Assert.That (ordering.Expression, Is.SameAs (expression));
      Assert.That (ordering.OrderingDirection, Is.EqualTo (directionAsc));
    }

    [Test]
    public void Accept()
    {
      var repository = new MockRepository();
      var queryModel = ExpressionHelper.CreateQueryModel_Cook ();
      var orderByClause = ExpressionHelper.CreateOrderByClause ();
      var visitorMock = repository.StrictMock<IQueryModelVisitor>();

      visitorMock.VisitOrdering (_ordering, queryModel, orderByClause, 1);

      repository.ReplayAll();

      _ordering.Accept (visitorMock, queryModel, orderByClause, 1);

      repository.VerifyAll();
    }

    [Test]
    public void Clone ()
    {
      var clone = _ordering.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_ordering));
      Assert.That (clone.Expression, Is.SameAs (_ordering.Expression));
      Assert.That (clone.OrderingDirection, Is.EqualTo (_ordering.OrderingDirection));
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = ExpressionHelper.CreateExpression ();
      var newExpression = ExpressionHelper.CreateExpression ();
      var ordering = new Ordering (oldExpression, OrderingDirection.Asc);

      ordering.TransformExpressions (ex =>
          {
            Assert.That (ex, Is.SameAs (oldExpression));
            return newExpression;
          });

      Assert.That (ordering.Expression, Is.SameAs (newExpression));
    }

    [Test]
    public new void ToString ()
    {
      var ordering1 = new Ordering (Expression.Constant (0), OrderingDirection.Asc);
      var ordering2 = new Ordering (Expression.Constant (0), OrderingDirection.Desc);

      Assert.That (ordering1.ToString (), Is.EqualTo ("0 asc"));
      Assert.That (ordering2.ToString (), Is.EqualTo ("0 desc"));
    }
  }
}
