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
using Remotion.Collections;
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
    private TestQueryModelVisitor _testVisitor;
    private WhereClause _bodyClauseMock1;
    private WhereClause _bodyClauseMock2;
    private JoinClause _joinClauseMock1;
    private JoinClause _joinClauseMock2;
    private Ordering _orderingMock1;
    private Ordering _orderingMock2;
    private ResultModificationBase _resultModificationMock1;
    private ResultModificationBase _resultModificationMock2;
    private QueryModel _queryModel;

    [SetUp]
    public void SetUp ()
    {
      _mockRepository = new MockRepository();
      _visitorMock = _mockRepository.StrictMock<QueryModelVisitorBase>();
      _testVisitor = new TestQueryModelVisitor ();

      _bodyClauseMock1 = _mockRepository.StrictMock<WhereClause> (ExpressionHelper.CreateExpression());
      _bodyClauseMock2 = _mockRepository.StrictMock<WhereClause> (ExpressionHelper.CreateExpression());

      _joinClauseMock1 = _mockRepository.StrictMock<JoinClause> (
          "x",
          typeof (Student),
          ExpressionHelper.CreateExpression(),
          ExpressionHelper.CreateExpression(),
          ExpressionHelper.CreateExpression());

      _joinClauseMock2 = _mockRepository.StrictMock<JoinClause> (
          "x",
          typeof (Student),
          ExpressionHelper.CreateExpression(),
          ExpressionHelper.CreateExpression(),
          ExpressionHelper.CreateExpression());

      _orderingMock1 = _mockRepository.StrictMock<Ordering> (ExpressionHelper.CreateExpression(), OrderingDirection.Asc);
      _orderingMock2 = _mockRepository.StrictMock<Ordering> (ExpressionHelper.CreateExpression(), OrderingDirection.Asc);

      _resultModificationMock1 = _mockRepository.StrictMock<ResultModificationBase> (CollectionExecutionStrategy.Instance);
      _resultModificationMock2 = _mockRepository.StrictMock<ResultModificationBase> (CollectionExecutionStrategy.Instance);

      _queryModel = ExpressionHelper.CreateQueryModel ();
    }

    [Test]
    public void VisitQueryModel ()
    {
      var mainFromClauseMock = _mockRepository.StrictMock<MainFromClause> ("x", typeof (Student), ExpressionHelper.CreateExpression());
      var selectClauseMock = _mockRepository.StrictMock<SelectClause> (ExpressionHelper.CreateExpression());
      var queryModel = new QueryModel (typeof (IQueryable<Student>), mainFromClauseMock, selectClauseMock);

      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.VisitQueryModel (queryModel))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        mainFromClauseMock.Expect (mock => mock.Accept (_visitorMock));
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitBodyClauses", queryModel, queryModel.BodyClauses));
        selectClauseMock.Expect (mock => mock.Accept (_visitorMock));
      }

      _mockRepository.ReplayAll();

      queryModel.Accept (_visitorMock);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitMainFromClause ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause();
      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.VisitMainFromClause (mainFromClause))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitJoinClauses", mainFromClause, mainFromClause.JoinClauses));
      }

      _visitorMock.Replay();

      _visitorMock.VisitMainFromClause (mainFromClause);

      _visitorMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitAdditionalFromClause ()
    {
      var additionalFromClause = ExpressionHelper.CreateAdditionalFromClause();
      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.VisitAdditionalFromClause (additionalFromClause, _queryModel, 1))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitJoinClauses", additionalFromClause, additionalFromClause.JoinClauses));
      }

      _visitorMock.Replay();

      _visitorMock.VisitAdditionalFromClause (additionalFromClause, _queryModel, 1);

      _visitorMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitOrderByClause ()
    {
      var orderByClause = ExpressionHelper.CreateOrderByClause();
      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.VisitOrderByClause (orderByClause, _queryModel, 1))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitOrderings", orderByClause, orderByClause.Orderings));
      }

      _visitorMock.Replay();

      _visitorMock.VisitOrderByClause (orderByClause, _queryModel, 1);

      _visitorMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSelectClause ()
    {
      var selectClause = ExpressionHelper.CreateSelectClause();
      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.VisitSelectClause (selectClause))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitResultModifications", selectClause, selectClause.ResultModifications));
      }

      _visitorMock.Replay();

      _visitorMock.VisitSelectClause (selectClause);

      _visitorMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitBodyClauses ()
    {
      var bodyClauses = new ObservableCollection<IBodyClause> { _bodyClauseMock1, _bodyClauseMock2 };

      using (_mockRepository.Ordered())
      {
        _bodyClauseMock1.Expect (mock => mock.Accept (Arg.Is (_testVisitor), Arg<QueryModel>.Is.Anything, Arg<int>.Is.Anything));
        _bodyClauseMock2.Expect (mock => mock.Accept (Arg.Is (_testVisitor), Arg<QueryModel>.Is.Anything, Arg<int>.Is.Anything));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitBodyClauses (_queryModel, bodyClauses);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitBodyClauses_WithChangingCollection ()
    {
      var bodyClauses = new ObservableCollection<IBodyClause> { _bodyClauseMock1, _bodyClauseMock2 };

      using (_mockRepository.Ordered())
      {
        _bodyClauseMock1
            .Expect (mock => mock.Accept (Arg.Is (_testVisitor), Arg<QueryModel>.Is.Anything, Arg<int>.Is.Anything))
            .WhenCalled (mi => bodyClauses.RemoveAt (0));
        _bodyClauseMock2
            .Expect (mock => mock.Accept (Arg.Is (_testVisitor), Arg<QueryModel>.Is.Anything, Arg<int>.Is.Anything));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitBodyClauses (_queryModel, bodyClauses);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitBodyClauses_Index_QueryModel ()
    {
      var bodyClauses = new ObservableCollection<IBodyClause> { _bodyClauseMock1, _bodyClauseMock2 };

      using (_mockRepository.Ordered ())
      {
        _bodyClauseMock1.Expect (mock => mock.Accept (_testVisitor, _queryModel, 0));
        _bodyClauseMock2.Expect (mock => mock.Accept (_testVisitor, _queryModel, 1));
      }

      _mockRepository.ReplayAll ();

      _testVisitor.VisitBodyClauses (_queryModel, bodyClauses);

      _mockRepository.VerifyAll ();
    }
    
    [Test]
    public void VisitJoinClauses ()
    {
      var joinClauses = new ObservableCollection<JoinClause> { _joinClauseMock1, _joinClauseMock2 };

      using (_mockRepository.Ordered())
      {
        _joinClauseMock1.Expect (mock => mock.Accept (_testVisitor));
        _joinClauseMock2.Expect (mock => mock.Accept (_testVisitor));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitJoinClauses (ExpressionHelper.CreateMainFromClause(), joinClauses);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitJoinClauses_WithChangingCollection ()
    {
      var joinClauses = new ObservableCollection<JoinClause> { _joinClauseMock1, _joinClauseMock2 };

      using (_mockRepository.Ordered())
      {
        _joinClauseMock1.Expect (mock => mock.Accept (_testVisitor)).WhenCalled (mi => joinClauses.RemoveAt (0));
        _joinClauseMock2.Expect (mock => mock.Accept (_testVisitor));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitJoinClauses (ExpressionHelper.CreateMainFromClause(), joinClauses);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitOrderings ()
    {
      var orderings = new ObservableCollection<Ordering> { _orderingMock1, _orderingMock2 };

      using (_mockRepository.Ordered())
      {
        _orderingMock1.Expect (mock => mock.Accept (_testVisitor));
        _orderingMock2.Expect (mock => mock.Accept (_testVisitor));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitOrderings (ExpressionHelper.CreateOrderByClause(), orderings);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitOrderings_WithChangingCollection ()
    {
      var orderings = new ObservableCollection<Ordering> { _orderingMock1, _orderingMock2 };

      using (_mockRepository.Ordered())
      {
        _orderingMock1.Expect (mock => mock.Accept (_testVisitor)).WhenCalled (mi => orderings.RemoveAt (0));
        _orderingMock2.Expect (mock => mock.Accept (_testVisitor));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitOrderings (ExpressionHelper.CreateOrderByClause(), orderings);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitResultModifications ()
    {
      var resultModifications = new ObservableCollection<ResultModificationBase> { _resultModificationMock1, _resultModificationMock2 };

      using (_mockRepository.Ordered())
      {
        _resultModificationMock1.Expect (mock => mock.Accept (_testVisitor));
        _resultModificationMock2.Expect (mock => mock.Accept (_testVisitor));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitResultModifications (ExpressionHelper.CreateSelectClause(), resultModifications);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitResultModifications_WithChangingCollection ()
    {
      var resultModifications = new ObservableCollection<ResultModificationBase> { _resultModificationMock1, _resultModificationMock2 };

      using (_mockRepository.Ordered())
      {
        _resultModificationMock1.Expect (mock => mock.Accept (_testVisitor)).WhenCalled (mi => resultModifications.RemoveAt (0));
        _resultModificationMock2.Expect (mock => mock.Accept (_testVisitor));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitResultModifications (ExpressionHelper.CreateSelectClause(), resultModifications);

      _mockRepository.VerifyAll();
    }
  }
}