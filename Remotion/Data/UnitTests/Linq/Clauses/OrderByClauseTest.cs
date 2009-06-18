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
using Remotion.Data.Linq.Clauses.Expressions;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;


namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class OrderByClauseTest
  {
    private ClonedClauseMapping _clonedClauseMapping;
    private OrderByClause _orderByClause;
    private IClause _previousClause;

    [SetUp]
    public void SetUp ()
    {
      _clonedClauseMapping = new ClonedClauseMapping();
      _previousClause = ExpressionHelper.CreateClause();
      _orderByClause = new OrderByClause (_previousClause);
    }

    [Test]
    public void InitializeWithoutOrdering()
    {
      Assert.AreEqual (0, _orderByClause.OrderingList.Count);
      Assert.IsNotNull (_orderByClause.PreviousClause);
      Assert.AreSame (_previousClause, _orderByClause.PreviousClause);
    }
    
    [Test]
    public void AddOrderings()
    {
      Ordering ordering1 = ExpressionHelper.CreateOrdering ();
      Ordering ordering2 = ExpressionHelper.CreateOrdering ();

      _orderByClause.AddOrdering (ordering1);
      _orderByClause.AddOrdering (ordering2);

      Assert.That (_orderByClause.OrderingList, Is.EqualTo (new object[] { ordering1, ordering2 }));
      Assert.AreEqual (2, _orderByClause.OrderingList.Count);

      Assert.IsNotNull (_orderByClause.PreviousClause);
    }

    [Test]
    public void Accept()
    {
      var repository = new MockRepository();
      var visitorMock = repository.StrictMock<IQueryVisitor>();

      visitorMock.VisitOrderByClause(_orderByClause);

      repository.ReplayAll();

      _orderByClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      Assert.IsNull (_orderByClause.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _orderByClause.SetQueryModel (model);
      Assert.IsNotNull (_orderByClause.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      _orderByClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _orderByClause.SetQueryModel (model);
      _orderByClause.SetQueryModel (model);
    }

    [Test]
    public void Clone ()
    {
      var newPreviousClause = ExpressionHelper.CreateClause ();
      var clone = _orderByClause.Clone (newPreviousClause, new ClonedClauseMapping());

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_orderByClause));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
      Assert.That (clone.QueryModel, Is.Null);
    }

    [Test]
    public void Clone_ViaInterface_PassesMapping ()
    {
      _clonedClauseMapping.AddMapping (_orderByClause.PreviousClause, ExpressionHelper.CreateClause ());
      var clone = ((IBodyClause) _orderByClause).Clone (ExpressionHelper.CreateClause (), _clonedClauseMapping);
      Assert.That (_clonedClauseMapping.GetClause (_orderByClause), Is.SameAs (clone));
    }

    [Test]
    public void Clone_Orderings ()
    {
      var ordering = ExpressionHelper.CreateOrdering();
      _orderByClause.AddOrdering (ordering);

      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      var clone = _orderByClause.Clone (newPreviousClause, new ClonedClauseMapping());

      Assert.That (clone.OrderingList.Count, Is.EqualTo (1));

      Assert.That (clone.OrderingList[0], Is.Not.SameAs (_orderByClause.OrderingList[0]));
      Assert.That (clone.OrderingList[0].Expression, Is.SameAs (_orderByClause.OrderingList[0].Expression));
      Assert.That (clone.OrderingList[0].OrderingDirection, Is.EqualTo (_orderByClause.OrderingList[0].OrderingDirection));
      Assert.That (clone.OrderingList[0].QueryModel, Is.Null);
      Assert.That (clone.OrderingList[0].OrderByClause, Is.SameAs (clone));
    }

    [Test]
    [Ignore("TODO 1229")]
    public void Clone_Orderings_PassesMapping ()
    {
      var newMainFromClause = ExpressionHelper.CreateMainFromClause ();

      var oldMainFromClause = ExpressionHelper.CreateMainFromClause();
      var ordering = new Ordering (_orderByClause, new QuerySourceReferenceExpression (oldMainFromClause), OrderingDirection.Asc);
      _orderByClause.AddOrdering (ordering);

      _clonedClauseMapping.AddMapping (oldMainFromClause, newMainFromClause);

      var clone = _orderByClause.Clone (newMainFromClause, _clonedClauseMapping);
      var clonedOrdering = clone.OrderingList[0];
      
      Assert.That (((QuerySourceReferenceExpression) clonedOrdering.Expression).ReferencedClause, Is.SameAs (newMainFromClause));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      var clone = _orderByClause.Clone (ExpressionHelper.CreateClause (), _clonedClauseMapping);
      Assert.That (_clonedClauseMapping.GetClause (_orderByClause), Is.SameAs (clone));
    }
  }
}
