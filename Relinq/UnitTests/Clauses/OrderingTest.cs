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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses
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
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
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
