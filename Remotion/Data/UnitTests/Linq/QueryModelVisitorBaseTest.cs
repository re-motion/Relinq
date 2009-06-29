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
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Development.UnitTesting;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using System.Linq;

namespace Remotion.Data.UnitTests.Linq
{
  [TestFixture]
  public class QueryModelVisitorBaseTest
  {
    private MockRepository _mockRepository;
    private QueryModelVisitorBase _visitorMock;

    [SetUp]
    public void SetUp ()
    {
      _mockRepository = new MockRepository();
      _visitorMock = _mockRepository.StrictMock<QueryModelVisitorBase> ();
    }

    [Test]
    public void VisitQueryModel ()
    {
      var mainFromClauseMock = _mockRepository.StrictMock<MainFromClause> ("x", typeof (Student), ExpressionHelper.CreateExpression());
      var selectClauseMock = _mockRepository.StrictMock<SelectClause> (ExpressionHelper.CreateExpression ());
      var queryModel = new QueryModel (typeof (IQueryable<Student>), mainFromClauseMock, selectClauseMock);

      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => mock.VisitQueryModel (queryModel))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        mainFromClauseMock.Expect (mock => mock.Accept (_visitorMock));
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitBodyClauses", queryModel, queryModel.BodyClauses));
        selectClauseMock.Expect (mock => mock.Accept (_visitorMock));
      }

      _mockRepository.ReplayAll ();

      queryModel.Accept (_visitorMock);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void VisitMainFromClause ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause ();
      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => mock.VisitMainFromClause (mainFromClause))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitJoinClauses", mainFromClause, mainFromClause.JoinClauses));
      }

      _visitorMock.Replay ();

      mainFromClause.Accept (_visitorMock);

      _visitorMock.VerifyAllExpectations ();
    }

    [Test]
    public void VisitAdditionalFromClause ()
    {
      var additionalFromClause = ExpressionHelper.CreateAdditionalFromClause ();
      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => mock.VisitAdditionalFromClause (additionalFromClause))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitJoinClauses", additionalFromClause, additionalFromClause.JoinClauses));
      }

      _visitorMock.Replay ();

      additionalFromClause.Accept (_visitorMock);

      _visitorMock.VerifyAllExpectations ();
    }

    [Test]
    public void VisitOrderByClause ()
    {
      var orderByClause = ExpressionHelper.CreateOrderByClause ();
      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => mock.VisitOrderByClause (orderByClause))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitOrderings", orderByClause, orderByClause.Orderings));
      }

      _visitorMock.Replay ();

      orderByClause.Accept (_visitorMock);

      _visitorMock.VerifyAllExpectations ();
    }

    [Test]
    public void VisitSelectClause ()
    {
      var selectClause = ExpressionHelper.CreateSelectClause ();
      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => mock.VisitSelectClause (selectClause))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitResultModifications", selectClause, selectClause.ResultModifications));
      }

      _visitorMock.Replay ();

      selectClause.Accept (_visitorMock);

      _visitorMock.VerifyAllExpectations ();
    }

    [Test]
    public void VisitBodyClauses ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel ();

      var bodyClauseMock1 = _mockRepository.StrictMock<WhereClause> (ExpressionHelper.CreateExpression());
      queryModel.BodyClauses.Add (bodyClauseMock1);

      var bodyClauseMock2 = _mockRepository.StrictMock<WhereClause> (ExpressionHelper.CreateExpression ());
      queryModel.BodyClauses.Add (bodyClauseMock2);

      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitBodyClauses", queryModel, queryModel.BodyClauses))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        bodyClauseMock1.Expect (mock => mock.Accept (_visitorMock));
        bodyClauseMock2.Expect (mock => mock.Accept (_visitorMock));
      }

      _mockRepository.ReplayAll ();

      PrivateInvoke.InvokeNonPublicMethod (_visitorMock, "VisitBodyClauses", queryModel, queryModel.BodyClauses);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void VisitBodyClauses_WithChangingCollection ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel ();
      var bodyClause1 = ExpressionHelper.CreateWhereClause ();
      var bodyClause2 = ExpressionHelper.CreateWhereClause ();
      queryModel.BodyClauses.Add (bodyClause1);
      queryModel.BodyClauses.Add (bodyClause2);

      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitBodyClauses", queryModel, queryModel.BodyClauses))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => mock.VisitWhereClause (bodyClause1)).WhenCalled (mi => queryModel.BodyClauses.RemoveAt (0));
        _visitorMock
            .Expect (mock => mock.VisitWhereClause (bodyClause2));
      }

      _mockRepository.ReplayAll ();

      PrivateInvoke.InvokeNonPublicMethod (_visitorMock, "VisitBodyClauses", queryModel, queryModel.BodyClauses);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void VisitJoinClauses ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause ();
      var joinClauseMock1 = _mockRepository.StrictMock<JoinClause> (
          "x", 
          typeof (Student), 
          ExpressionHelper.CreateExpression (), 
          ExpressionHelper.CreateExpression (), 
          ExpressionHelper.CreateExpression ());
      mainFromClause.JoinClauses.Add (joinClauseMock1);

      var joinClauseMock2 = _mockRepository.StrictMock<JoinClause> (
          "x",
          typeof (Student),
          ExpressionHelper.CreateExpression (),
          ExpressionHelper.CreateExpression (),
          ExpressionHelper.CreateExpression ());
      mainFromClause.JoinClauses.Add (joinClauseMock2);

      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitJoinClauses", mainFromClause, mainFromClause.JoinClauses))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        joinClauseMock1.Expect (mock => mock.Accept (_visitorMock));
        joinClauseMock2.Expect (mock => mock.Accept (_visitorMock));
      }

      _mockRepository.ReplayAll ();

      PrivateInvoke.InvokeNonPublicMethod (_visitorMock, "VisitJoinClauses", mainFromClause, mainFromClause.JoinClauses);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void VisitJoinClauses_WithChangingCollection ()
    {
      var fromClause = ExpressionHelper.CreateMainFromClause ();
      var joinClause1 = ExpressionHelper.CreateJoinClause ();
      var joinClause2 = ExpressionHelper.CreateJoinClause ();
      fromClause.JoinClauses.Add (joinClause1);
      fromClause.JoinClauses.Add (joinClause2);

      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitJoinClauses", fromClause, fromClause.JoinClauses))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => mock.VisitJoinClause (joinClause1)).WhenCalled (mi => fromClause.JoinClauses.RemoveAt (0));
        _visitorMock
            .Expect (mock => mock.VisitJoinClause (joinClause2));
      }

      _mockRepository.ReplayAll ();

      PrivateInvoke.InvokeNonPublicMethod (_visitorMock, "VisitJoinClauses", fromClause, fromClause.JoinClauses);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void VisitOrderings ()
    {
      var orderByClause = ExpressionHelper.CreateOrderByClause ();

      var orderingMock1 = _mockRepository.StrictMock<Ordering> (ExpressionHelper.CreateExpression (), OrderingDirection.Asc);
      orderByClause.Orderings.Add (orderingMock1);

      var orderingMock2 = _mockRepository.StrictMock<Ordering> (ExpressionHelper.CreateExpression (), OrderingDirection.Asc);
      orderByClause.Orderings.Add (orderingMock2);

      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitOrderings", orderByClause, orderByClause.Orderings))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        orderingMock1.Expect (mock => mock.Accept (_visitorMock));
        orderingMock2.Expect (mock => mock.Accept (_visitorMock));
      }

      _mockRepository.ReplayAll ();

      PrivateInvoke.InvokeNonPublicMethod (_visitorMock, "VisitOrderings", orderByClause, orderByClause.Orderings);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void VisitOrderings_WithChangingCollection ()
    {
      var orderByClause = ExpressionHelper.CreateOrderByClause ();
      var ordering1 = ExpressionHelper.CreateOrdering ();
      var ordering2 = ExpressionHelper.CreateOrdering ();
      orderByClause.Orderings.Add (ordering1);
      orderByClause.Orderings.Add (ordering2);
     
      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitOrderings", orderByClause, orderByClause.Orderings))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => mock.VisitOrdering (ordering1)).WhenCalled (mi => orderByClause.Orderings.RemoveAt(0));
        _visitorMock
            .Expect (mock => mock.VisitOrdering(ordering2));
      }

      _mockRepository.ReplayAll ();

      PrivateInvoke.InvokeNonPublicMethod (_visitorMock, "VisitOrderings", orderByClause, orderByClause.Orderings);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void VisitResultModifications ()
    {
      var selectClause = ExpressionHelper.CreateSelectClause ();

      var resultModificationMock1 = _mockRepository.StrictMock<ResultModificationBase> (CollectionExecutionStrategy.Instance);
      selectClause.ResultModifications.Add (resultModificationMock1);

      var resultModificationMock2 = _mockRepository.StrictMock<ResultModificationBase> (CollectionExecutionStrategy.Instance);
      selectClause.ResultModifications.Add (resultModificationMock2);

      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitResultModifications", selectClause, selectClause.ResultModifications))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        resultModificationMock1.Expect (mock => mock.Accept (_visitorMock));
        resultModificationMock2.Expect (mock => mock.Accept (_visitorMock));
      }

      _mockRepository.ReplayAll ();

      PrivateInvoke.InvokeNonPublicMethod (_visitorMock, "VisitResultModifications", selectClause, selectClause.ResultModifications);

      _mockRepository.VerifyAll ();
    }

    [Test]
    public void VisitResultModifications_WithChangingCollection ()
    {
      var selectClause = ExpressionHelper.CreateSelectClause ();
      var resultModification1 = ExpressionHelper.CreateResultModification ();
      var resultModification2 = ExpressionHelper.CreateResultModification ();
      selectClause.ResultModifications.Add (resultModification1);
      selectClause.ResultModifications.Add (resultModification2);
      
      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitResultModifications", selectClause, selectClause.ResultModifications))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => mock.VisitResultModification (resultModification1)).WhenCalled (mi => selectClause.ResultModifications.RemoveAt (0));
        _visitorMock
            .Expect (mock => mock.VisitResultModification (resultModification2));
      }

      _mockRepository.ReplayAll ();

      PrivateInvoke.InvokeNonPublicMethod (_visitorMock, "VisitResultModifications", selectClause, selectClause.ResultModifications);

      _mockRepository.VerifyAll ();
    }


  }
}