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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.UnitTests.TestDomain;
using Remotion.Linq.UnitTests.TestQueryGenerators;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests
{
  [TestFixture]
  public class QueryProviderBaseTest
  {
    private MockRepository _mockRepository;
    private IQueryParser _queryParserMock;
    private IQueryExecutor _executorMock;

    // private TestQueryProvider _queryProvider;
    private TestQueryProvider _queryProvider;

    private TestQueryable<Cook> _queryableWithDefaultParser;
    private QueryModel _fakeQueryModel;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      _queryParserMock = _mockRepository.StrictMock<IQueryParser> ();
      _executorMock = _mockRepository.StrictMock<IQueryExecutor> ();

      _queryProvider = new TestQueryProvider (_queryParserMock, _executorMock);
    
      _queryableWithDefaultParser = new TestQueryable<Cook> (QueryParser.CreateDefault(), _executorMock);
      _fakeQueryModel = ExpressionHelper.CreateQueryModel<Cook> ();
    }

    [Test]
    public void Initialization()
    {
      Assert.That (_queryProvider.Executor, Is.SameAs (_executorMock));
      Assert.That (_queryProvider.QueryParser, Is.SameAs (_queryParserMock));
    }

    [Test]
    public void CreateQueryReturnsIQueryable()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      IQueryable queryable = _queryProvider.CreateQuery (expression);

      Assert.That (queryable, Is.Not.Null);
      Assert.IsInstanceOf (typeof (IQueryable<int>), queryable);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "Expected a type implementing IEnumerable<T>, but found 'System.Func`1[System.Int32]'.\r\nParameter name: expression")]
    public void CreateQuery_NonEnumerableExpression ()
    {
      Expression expression = ExpressionHelper.CreateLambdaExpression ();
      _queryProvider.CreateQuery (expression);
    }

    [Test]
    public void GenericCreateQueryReturnsIQueryable ()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression ();
      IQueryable<int> queryable = _queryProvider.CreateQuery<int> (expression);

      Assert.That (queryable, Is.Not.Null);
    }

    [Test]
    public void GenerateQueryModel ()
    {
      Expression expression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQueryable<Cook> ());

      _queryParserMock
          .Expect (mock => mock.GetParsedQuery (expression))
          .Return (_fakeQueryModel);
      _queryParserMock.Replay ();

      var queryModel = _queryProvider.GenerateQueryModel (expression);

      _queryParserMock.VerifyAllExpectations();
      Assert.That (queryModel, Is.SameAs (_fakeQueryModel));
    }

    [Test]
    public void Execute ()
    {
      var expectedResult = new Cook[0];
      Expression expression = (from s in _queryableWithDefaultParser select s).Expression;

      _queryParserMock
          .Expect (mock => mock.GetParsedQuery (expression))
          .Return (_fakeQueryModel);
      _queryParserMock.Replay ();

      _executorMock
          .Expect (mock => mock.ExecuteCollection<Cook> (_fakeQueryModel))
          .Return (expectedResult);
      _executorMock.Replay ();

      var result = _queryProvider.Execute (expression);

      _queryParserMock.VerifyAllExpectations ();
      _executorMock.VerifyAllExpectations ();
      Assert.That (result.Value, Is.EqualTo (expectedResult));
    }

    [Test]
    public void Execute_Explicit_Generic ()
    {
      var expectedResult = new Cook[0];
      Expression expression = (from s in _queryableWithDefaultParser select s).Expression;

      _queryParserMock
          .Expect (mock => mock.GetParsedQuery (expression))
          .Return (_fakeQueryModel);
      _queryParserMock.Replay();

      _executorMock
          .Expect (mock => mock.ExecuteCollection<Cook> (_fakeQueryModel))
          .Return (expectedResult);
      _executorMock.Replay ();

      var result = ((IQueryProvider) _queryProvider).Execute<IEnumerable<Cook>> (expression);

      _queryParserMock.VerifyAllExpectations();
      _executorMock.VerifyAllExpectations ();
      Assert.That (result.ToArray(), Is.EqualTo (expectedResult));
    }

    [Test]
    public void Execute_Explicit_NonGeneric ()
    {
      var expectedResult = new Cook[0];
      Expression expression = (from s in _queryableWithDefaultParser select s).Expression;
      
      _queryParserMock
          .Expect (mock => mock.GetParsedQuery (expression))
          .Return (_fakeQueryModel);
      _queryParserMock.Replay ();

      _executorMock
          .Expect (mock => mock.ExecuteCollection<Cook> (_fakeQueryModel))
          .Return (new Cook[0]);
      _executorMock.Replay ();

      var result = ((IQueryProvider)_queryProvider).Execute (expression);

      _queryParserMock.VerifyAllExpectations();
      _executorMock.VerifyAllExpectations ();
      Assert.That (((IEnumerable<Cook>)result).ToArray (), Is.EqualTo (expectedResult));
    }

    [Test]
    public void Execute_Explicit_NonGeneric_WithExpressionTreeReturningSpecificQueryableType ()
    {
      var expectedResult = new Cook[0];
      Expression expression = _queryableWithDefaultParser.Expression;

      _queryParserMock
          .Expect (mock => mock.GetParsedQuery (expression))
          .Return (_fakeQueryModel);
      _queryParserMock.Replay ();

      _executorMock
          .Expect (mock => mock.ExecuteCollection<Cook> (_fakeQueryModel))
          .Return (new Cook[0]);
      _executorMock.Replay ();

      var result = ((IQueryProvider) _queryProvider).Execute (expression);

      _queryParserMock.VerifyAllExpectations ();
      _executorMock.VerifyAllExpectations ();
      Assert.That (((IEnumerable<Cook>) result).ToArray (), Is.EqualTo (expectedResult));
    }

    [Test]
    public void Execute_IntegrationWithGenericEnumerable ()
    {
      var queryable = from s in _queryableWithDefaultParser select s;

      var expectedResult = new[] { new Cook () };
      _executorMock
          .Expect (mock => mock.ExecuteCollection<Cook> (Arg<QueryModel>.Is.Anything))
          .Return (expectedResult);

      _executorMock.Replay ();
      var result = queryable.ToArray ();
      _executorMock.VerifyAllExpectations ();

      Assert.That (result, Is.EqualTo (expectedResult));
    }

    [Test]
    public void Execute_IntegrationWithNonGenericEnumerable ()
    {
      var queryable = from s in _queryableWithDefaultParser select s;

      var expectedResult = new[] { new Cook () };
      _executorMock
          .Expect (mock => mock.ExecuteCollection<Cook> (Arg<QueryModel>.Is.Anything))
          .Return (expectedResult);

      _executorMock.Replay ();

      var result = new List<object> ();
      foreach (var s in (IEnumerable) queryable)
        result.Add (s);

      _executorMock.VerifyAllExpectations ();

      Assert.That (result, Is.EqualTo (expectedResult));
    }

    [Test]
    public void Execute_IntegrationWithNonGenericEnumerable_ExpressionReturnsQueryableType ()
    {
      var queryable = _queryableWithDefaultParser;

      var expectedResult = new[] { new Cook () };
      _executorMock
          .Expect (mock => mock.ExecuteCollection<Cook> (Arg<QueryModel>.Is.Anything))
          .Return (expectedResult);

      _executorMock.Replay ();

      var result = new List<object> ();
      foreach (var s in (IEnumerable) queryable)
        result.Add (s);

      _executorMock.VerifyAllExpectations ();

      Assert.That (result, Is.EqualTo (expectedResult));
    }

    [Test]
    public void Execute_IntegrationWithSingle ()
    {
      var expectedResult = new Cook ();
      _executorMock
                .Expect (mock => mock.ExecuteSingle<Cook> (Arg<QueryModel>.Is.Anything, Arg<bool>.Is.Equal (false)))
                .Return (expectedResult);

      _executorMock.Replay ();
      var result = (from s in _queryableWithDefaultParser select s).Single();
      _executorMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs(expectedResult));
    }

    [Test]
    public void Execute_IntegrationWithScalar ()
    {
      _executorMock
                .Expect (mock => mock.ExecuteScalar<int> (Arg<QueryModel>.Is.Anything))
                .Return (7);

      _executorMock.Replay ();
      var result = (from s in _queryableWithDefaultParser select s).Count ();
      _executorMock.VerifyAllExpectations();

      Assert.That (result, Is.EqualTo (7));
    }
  }
}
