/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using OrderDirection=Remotion.Data.Linq.Clauses.OrderDirection;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class OrderingClauseTest
  {
   
    [Test]
    public void InitializeWithExpressionAndOrderDirectionAsc()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression();
      OrderDirection directionAsc  = OrderDirection.Asc;

      IClause clause = ExpressionHelper.CreateClause();
      
      OrderingClause ordering = new OrderingClause(clause, expression,directionAsc);


      Assert.AreSame (clause, ordering.PreviousClause);
      Assert.AreSame (expression, ordering.Expression);
      Assert.AreEqual (directionAsc, ordering.OrderDirection);
    }

    [Test]
    public void InitializeWithExpressionAndOrderDirectionDesc ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      OrderDirection directionAsc = OrderDirection.Asc;

      IClause clause = ExpressionHelper.CreateClause ();

      OrderingClause ordering = new OrderingClause (clause,expression, directionAsc);

      Assert.AreSame (clause, ordering.PreviousClause);
      Assert.AreSame (expression, ordering.Expression);
      Assert.AreEqual (directionAsc, ordering.OrderDirection);
    }

    [Test]
    public void OrderingClause_ImplementsIQueryElement()
    {
      OrderingClause orderingClause = ExpressionHelper.CreateOrderingClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), orderingClause);
    }

    [Test]
    public void Accept()
    {
      OrderingClause orderingClause = ExpressionHelper.CreateOrderingClause ();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.StrictMock<IQueryVisitor>();

      visitorMock.VisitOrderingClause (orderingClause);

      repository.ReplayAll();

      orderingClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      OrderingClause orderingClause = ExpressionHelper.CreateOrderingClause ();
      Assert.IsNull (orderingClause.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      OrderingClause orderingClause = ExpressionHelper.CreateOrderingClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      orderingClause.SetQueryModel (model);
      Assert.IsNotNull (orderingClause.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      OrderingClause orderingClause = ExpressionHelper.CreateOrderingClause ();
      orderingClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      OrderingClause orderingClause = ExpressionHelper.CreateOrderingClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      orderingClause.SetQueryModel (model);
      orderingClause.SetQueryModel (model);
    }
  }
}
