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
#if !NET_3_5
using System.Collections.ObjectModel;
#endif
using NUnit.Framework;
using Remotion.Linq.Clauses;
#if NET_3_5
using Remotion.Linq.Collections;
#endif
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Clauses.ResultOperators;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.Linq.UnitTests
{
  [TestFixture]
  public class QueryModelVisitorBaseTest
  {
    private MockRepository _mockRepository;
    private TestQueryModelVisitor _visitorMock;
    private IBodyClause _bodyClauseMock1;
    private IBodyClause _bodyClauseMock2;
    private Ordering _ordering1;
    private Ordering _ordering2;
    private ResultOperatorBase _resultOperator1;
    private ResultOperatorBase _resultOperator2;
    private QueryModel _queryModel;
    private OrderByClause _orderByClause;
    private GroupJoinClause _groupJoinClause;

    [SetUp]
    public void SetUp ()
    {
      _mockRepository = new MockRepository();
      _visitorMock = _mockRepository.StrictMock<TestQueryModelVisitor>();

      _bodyClauseMock1 = _mockRepository.StrictMock<IBodyClause>();
      _bodyClauseMock2 = _mockRepository.StrictMock<IBodyClause>();

      _ordering1 = new Ordering (ExpressionHelper.CreateExpression(), OrderingDirection.Asc);
      _ordering2 = new Ordering (ExpressionHelper.CreateExpression(), OrderingDirection.Asc);

      _resultOperator1 = new TestResultOperator();
      _resultOperator2 = new TestResultOperator();

      _queryModel = ExpressionHelper.CreateQueryModel<Cook>();
      _orderByClause = ExpressionHelper.CreateOrderByClause();
      _groupJoinClause = ExpressionHelper.CreateGroupJoinClause<Cook>();
    }

    [Test]
    public void VisitQueryModel ()
    {
      var mainFromClaus = new MainFromClause ("x", typeof (Cook), ExpressionHelper.CreateExpression());
      var selectClause =  new SelectClause (ExpressionHelper.CreateExpression());
      var queryModel = new QueryModel (mainFromClaus, selectClause);

      using (_mockRepository.Ordered())
      {
        _visitorMock.Expect (mock => mock.VisitQueryModel (queryModel)).CallOriginalMethod (OriginalCallOptions.NoExpectation);
        _visitorMock.Expect (mock => mock.VisitMainFromClause (mainFromClaus, queryModel));
        _visitorMock.Expect (mock => mock.InvokeVisitBodyClauses (queryModel.BodyClauses, queryModel));
        _visitorMock.Expect (mock => mock.VisitSelectClause (selectClause, queryModel));
        _visitorMock.Expect (mock => mock.InvokeVisitResultOperators (queryModel.ResultOperators, queryModel));
      }

      _mockRepository.ReplayAll();

      _visitorMock.VisitQueryModel (queryModel);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitOrderByClause ()
    {
      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.VisitOrderByClause (_orderByClause, _queryModel, 1))
            .CallOriginalMethod (OriginalCallOptions.NoExpectation);
        _visitorMock
            .Expect (mock => mock.InvokeVisitOrderings (_orderByClause.Orderings, _queryModel, _orderByClause));
      }

      _visitorMock.Replay();

      _visitorMock.VisitOrderByClause (_orderByClause, _queryModel, 1);

      _visitorMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitGroupJoinClause ()
    {
      using (_mockRepository.Ordered ())
      {
        _visitorMock
            .Expect (mock => mock.VisitGroupJoinClause (_groupJoinClause, _queryModel, 1))
            .CallOriginalMethod (OriginalCallOptions.NoExpectation);
        _visitorMock
            .Expect (mock => mock.VisitJoinClause (_groupJoinClause.JoinClause, _queryModel, _groupJoinClause));
      }

      _visitorMock.Replay ();

      _visitorMock.VisitGroupJoinClause (_groupJoinClause, _queryModel, 1);

      _visitorMock.VerifyAllExpectations ();
    }

    [Test]
    public void VisitBodyClauses ()
    {
      var bodyClauses = new ObservableCollection<IBodyClause> { _bodyClauseMock1, _bodyClauseMock2 };

      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.InvokeVisitBodyClauses (bodyClauses, _queryModel))
            .CallOriginalMethod (OriginalCallOptions.NoExpectation);
        _bodyClauseMock1
            .Expect (mock => mock.Accept (_visitorMock, _queryModel, 0));
        _bodyClauseMock2
            .Expect (mock => mock.Accept (_visitorMock, _queryModel, 1));
      }

      _mockRepository.ReplayAll();

      _visitorMock.InvokeVisitBodyClauses (bodyClauses, _queryModel);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitBodyClauses_WithChangingCollection ()
    {
      var bodyClauses = new ObservableCollection<IBodyClause> { _bodyClauseMock1, _bodyClauseMock2 };

      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.InvokeVisitBodyClauses (bodyClauses, _queryModel))
            .CallOriginalMethod (OriginalCallOptions.NoExpectation);
        _bodyClauseMock1
            .Expect (mock => mock.Accept (_visitorMock, _queryModel, 0))
            .WhenCalled (mi => bodyClauses.RemoveAt (0));
        _bodyClauseMock2
            .Expect (mock => mock.Accept (_visitorMock, _queryModel, 0));
      }

      _mockRepository.ReplayAll();

      _visitorMock.InvokeVisitBodyClauses (bodyClauses, _queryModel);

      _mockRepository.VerifyAll();
    }
    
    [Test]
    public void VisitOrderings ()
    {
      var orderings = new ObservableCollection<Ordering> { _ordering1, _ordering2 };

      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.InvokeVisitOrderings (orderings, _queryModel, _orderByClause))
            .CallOriginalMethod (OriginalCallOptions.NoExpectation);
        _visitorMock
            .Expect (mock => mock.VisitOrdering (_ordering1, _queryModel, _orderByClause, 0));
        _visitorMock
            .Expect (mock => mock.VisitOrdering (_ordering2, _queryModel, _orderByClause, 1));
      }

      _mockRepository.ReplayAll();

      _visitorMock.InvokeVisitOrderings (orderings, _queryModel, _orderByClause);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitOrderings_WithChangingCollection ()
    {
      var orderings = new ObservableCollection<Ordering> { _ordering1, _ordering2 };

      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.InvokeVisitOrderings (orderings, _queryModel, _orderByClause))
            .CallOriginalMethod (OriginalCallOptions.NoExpectation);
        _visitorMock
            .Expect (mock => mock.VisitOrdering (_ordering1, _queryModel, _orderByClause, 0))
            .WhenCalled (mi => orderings.RemoveAt (0));
        _visitorMock
            .Expect (mock => mock.VisitOrdering (_ordering2, _queryModel, _orderByClause, 0));
      }

      _mockRepository.ReplayAll();

      _visitorMock.InvokeVisitOrderings (orderings, _queryModel, _orderByClause);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitResultOperators ()
    {
      var resultOperators = new ObservableCollection<ResultOperatorBase> { _resultOperator1, _resultOperator2 };

      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.InvokeVisitResultOperators (resultOperators, _queryModel))
            .CallOriginalMethod (OriginalCallOptions.NoExpectation);
        _visitorMock
          .Expect (mock => mock.VisitResultOperator (_resultOperator1, _queryModel, 0));
        _visitorMock
          .Expect (mock => mock.VisitResultOperator (_resultOperator2, _queryModel, 1));
      }

      _mockRepository.ReplayAll();

      _visitorMock.InvokeVisitResultOperators (resultOperators, _queryModel);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitResultOperators_WithChangingCollection ()
    {
      var resultOperators = new ObservableCollection<ResultOperatorBase> { _resultOperator1, _resultOperator2 };

      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.InvokeVisitResultOperators (resultOperators, _queryModel))
            .CallOriginalMethod (OriginalCallOptions.NoExpectation);
        _visitorMock
            .Expect (mock => mock.VisitResultOperator (_resultOperator1, _queryModel, 0))
            .WhenCalled (mi => resultOperators.RemoveAt (0));
        _visitorMock
            .Expect (mock => mock.VisitResultOperator (_resultOperator2, _queryModel, 0));
      }

      _mockRepository.ReplayAll();

      _visitorMock.InvokeVisitResultOperators (resultOperators, _queryModel);

      _mockRepository.VerifyAll();
    }
  }
}
