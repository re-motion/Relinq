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
using System.Collections.ObjectModel;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Linq.Clauses;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.Linq.UnitTests
{
  [TestFixture]
  public class QueryModelVisitorBaseTest
  {
    private MockRepository _mockRepository;
    private QueryModelVisitorBase _visitorMock;
    private TestQueryModelVisitor _testVisitor;
    private WhereClause _bodyClauseMock1;
    private WhereClause _bodyClauseMock2;
    private Ordering _orderingMock1;
    private Ordering _orderingMock2;
    private ResultOperatorBase _resultOperatorMock1;
    private ResultOperatorBase _resultOperatorMock2;
    private QueryModel _queryModel;
    private OrderByClause _orderByClause;
    private GroupJoinClause _groupJoinClause;

    [SetUp]
    public void SetUp ()
    {
      _mockRepository = new MockRepository();
      _visitorMock = _mockRepository.StrictMock<QueryModelVisitorBase>();
      _testVisitor = new TestQueryModelVisitor ();

      _bodyClauseMock1 = _mockRepository.StrictMock<WhereClause> (ExpressionHelper.CreateExpression());
      _bodyClauseMock2 = _mockRepository.StrictMock<WhereClause> (ExpressionHelper.CreateExpression());

      _orderingMock1 = _mockRepository.StrictMock<Ordering> (ExpressionHelper.CreateExpression(), OrderingDirection.Asc);
      _orderingMock2 = _mockRepository.StrictMock<Ordering> (ExpressionHelper.CreateExpression(), OrderingDirection.Asc);

      _resultOperatorMock1 = _mockRepository.StrictMock<ResultOperatorBase> ();
      _resultOperatorMock2 = _mockRepository.StrictMock<ResultOperatorBase> ();

      _queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      _orderByClause = ExpressionHelper.CreateOrderByClause ();
      _groupJoinClause = ExpressionHelper.CreateGroupJoinClause<Cook> ();
    }

    [Test]
    public void VisitQueryModel ()
    {
      var mainFromClauseMock = _mockRepository.StrictMock<MainFromClause> ("x", typeof (Cook), ExpressionHelper.CreateExpression());
      var selectClauseMock = _mockRepository.StrictMock<SelectClause> (ExpressionHelper.CreateExpression());
      var queryModel = new QueryModel (mainFromClauseMock, selectClauseMock);

      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.VisitQueryModel (queryModel))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        mainFromClauseMock.Expect (mock => mock.Accept (_visitorMock, queryModel));
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitBodyClauses", queryModel.BodyClauses, queryModel));
        selectClauseMock.Expect (mock => mock.Accept (_visitorMock, queryModel));
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitResultOperators", queryModel.ResultOperators, queryModel));
      }

      _mockRepository.ReplayAll();

      queryModel.Accept (_visitorMock);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitOrderByClause ()
    {
      using (_mockRepository.Ordered())
      {
        _visitorMock
            .Expect (mock => mock.VisitOrderByClause (_orderByClause, _queryModel, 1))
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        _visitorMock
            .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitOrderings", _orderByClause.Orderings, _queryModel, _orderByClause));
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
            .CallOriginalMethod (OriginalCallOptions.CreateExpectation);
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
        _bodyClauseMock1.Expect (mock => mock.Accept (_testVisitor, _queryModel, 0));
        _bodyClauseMock2.Expect (mock => mock.Accept (_testVisitor, _queryModel, 1));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitBodyClauses (bodyClauses, _queryModel);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitBodyClauses_WithChangingCollection ()
    {
      var bodyClauses = new ObservableCollection<IBodyClause> { _bodyClauseMock1, _bodyClauseMock2 };

      using (_mockRepository.Ordered())
      {
        _bodyClauseMock1
            .Expect (mock => mock.Accept (_testVisitor, _queryModel, 0))
            .WhenCalled (mi => bodyClauses.RemoveAt (0));
        _bodyClauseMock2
            .Expect (mock => mock.Accept (_testVisitor, _queryModel, 0));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitBodyClauses (bodyClauses, _queryModel);

      _mockRepository.VerifyAll();
    }
    
    [Test]
    public void VisitOrderings ()
    {
      var orderings = new ObservableCollection<Ordering> { _orderingMock1, _orderingMock2 };

      using (_mockRepository.Ordered())
      {
        _orderingMock1.Expect (mock => mock.Accept (_testVisitor, _queryModel, _orderByClause, 0));
        _orderingMock2.Expect (mock => mock.Accept (_testVisitor, _queryModel, _orderByClause, 1));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitOrderings (orderings, _queryModel, _orderByClause);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitOrderings_WithChangingCollection ()
    {
      var orderings = new ObservableCollection<Ordering> { _orderingMock1, _orderingMock2 };

      using (_mockRepository.Ordered())
      {
        _orderingMock1.Expect (mock => mock.Accept (_testVisitor, _queryModel, _orderByClause, 0)).WhenCalled (mi => orderings.RemoveAt (0));
        _orderingMock2.Expect (mock => mock.Accept (_testVisitor, _queryModel, _orderByClause, 0));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitOrderings (orderings, _queryModel, _orderByClause);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitResultOperators ()
    {
      var resultOperators = new ObservableCollection<ResultOperatorBase> { _resultOperatorMock1, _resultOperatorMock2 };

      using (_mockRepository.Ordered())
      {
        _resultOperatorMock1.Expect (mock => mock.Accept (_testVisitor, _queryModel, 0));
        _resultOperatorMock2.Expect (mock => mock.Accept (_testVisitor, _queryModel, 1));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitResultOperators (resultOperators, _queryModel);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void VisitResultOperators_WithChangingCollection ()
    {
      var resultOperators = new ObservableCollection<ResultOperatorBase> { _resultOperatorMock1, _resultOperatorMock2 };

      using (_mockRepository.Ordered())
      {
        _resultOperatorMock1
            .Expect (mock => mock.Accept (_testVisitor, _queryModel, 0))
            .WhenCalled (mi => resultOperators.RemoveAt (0));
        _resultOperatorMock2
            .Expect (mock => mock.Accept (_testVisitor, _queryModel, 0));
      }

      _mockRepository.ReplayAll();

      _testVisitor.VisitResultOperators (resultOperators, _queryModel);

      _mockRepository.VerifyAll();
    }
  }
}
