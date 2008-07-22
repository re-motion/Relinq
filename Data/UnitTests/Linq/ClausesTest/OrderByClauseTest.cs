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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;


namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class OrderByClauseTest
  {
    [Test]
    public void InitializeWithOneOrdering()
    {
      OrderingClause ordering = ExpressionHelper.CreateOrderingClause();
      OrderByClause orderBy = new OrderByClause (ordering);

      Assert.AreEqual (1, orderBy.OrderingList.Count);
      Assert.IsNotNull (orderBy.PreviousClause);
      Assert.AreSame (orderBy.OrderingList[0].PreviousClause, orderBy.PreviousClause);
    }
    
    [Test]
    public void AddMoreOrderings()
    {
      OrderingClause ordering1 = ExpressionHelper.CreateOrderingClause ();
      OrderingClause ordering2 = ExpressionHelper.CreateOrderingClause ();
      OrderByClause orderBy = new OrderByClause (ordering1);
      orderBy.Add (ordering2);

      Assert.That (orderBy.OrderingList, Is.EqualTo (new object[] { ordering1, ordering2 }));
      Assert.AreEqual (2, orderBy.OrderingList.Count);

      Assert.IsNotNull (orderBy.PreviousClause);
      Assert.AreSame (orderBy.OrderingList[0].PreviousClause, orderBy.PreviousClause);
    }

    [Test]
    public void OrderByClause_ImplementsIQueryElement()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), orderByClause);
    }

    [Test]
    public void Accept()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause ();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();

      visitorMock.VisitOrderByClause(orderByClause);

      repository.ReplayAll();

      orderByClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause ();
      Assert.IsNull (orderByClause.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      orderByClause.SetQueryModel (model);
      Assert.IsNotNull (orderByClause.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause ();

      orderByClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      orderByClause.SetQueryModel (model);
      orderByClause.SetQueryModel (model);
    }
  }
}
