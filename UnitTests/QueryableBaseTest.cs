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
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests
{
  [TestFixture]
  public class QueryableBaseTest
  {
    private IQueryProvider _providerMock;
    private MockRepository _mockRepository;
    private Expression _intArrayExpression;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      _providerMock = _mockRepository.StrictMock<IQueryProvider> ();

      _intArrayExpression = ExpressionHelper.CreateNewIntArrayExpression ();
    }

    [Test]
    public void Initialize_WithProviderAndExpression ()
    {
      QueryableBase<int> queryable = new TestQueryable<int> (_providerMock, _intArrayExpression);

      Assert.That (queryable.Provider, Is.SameAs (_providerMock));
      Assert.That (queryable.Expression, Is.SameAs (_intArrayExpression));

      Assert.That (queryable.ElementType, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void Initialize_WithProvider ()
    {
      QueryableBase<int> queryable = new TestQueryable<int> (_providerMock);

      Assert.That (queryable.Provider, Is.SameAs (_providerMock));
      Assert.That (queryable.Expression, Is.Not.Null);
      Assert.That (queryable.Expression.NodeType, Is.EqualTo (ExpressionType.Constant));
    }

    [Test]
    public void Initialize_WithParserAndExecutor ()
    {
      var fakeExecutor = _mockRepository.Stub<IQueryExecutor>();
      var fakeParser = _mockRepository.Stub<IQueryParser> ();
      QueryableBase<int> queryable = new TestQueryable<int> (fakeParser, fakeExecutor);

      Assert.That (queryable.Expression, Is.Not.Null);
      Assert.That (queryable.Expression.NodeType, Is.EqualTo (ExpressionType.Constant));
      Assert.That (queryable.Provider, Is.InstanceOf (typeof (DefaultQueryProvider)));

      var queryProvider = ((DefaultQueryProvider) queryable.Provider);
      Assert.That (queryProvider.Executor, Is.SameAs (fakeExecutor));
      Assert.That (queryProvider.QueryableType, Is.SameAs (typeof (TestQueryable<>)));
      Assert.That (queryProvider.QueryParser, Is.SameAs (fakeParser));
    }

    [Test]
    public void ConstructorThrowsTypeException ()
    {
      Assert.That (
          () => new TestQueryable<string> (_providerMock, _intArrayExpression),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Parameter 'expression' is a 'System.Int32[]', which cannot be assigned to type 'System.Collections.Generic.IEnumerable`1[System.String]'."
                  + "\r\nParameter name: expression"));
    }

    [Test]
    public void GenericGetEnumerator ()
    {
      _providerMock.Expect (mock => mock.Execute<IEnumerable<int>> (_intArrayExpression)).Return (new List<int>(0));

      _providerMock.Replay ();
      QueryableBase<int> queryable = new TestQueryable<int> (_providerMock, _intArrayExpression);
      queryable.GetEnumerator();
      _providerMock.VerifyAllExpectations();
    }

    [Test]
    public void GetEnumerator()
    {
      _providerMock.Expect (mock => mock.Execute<IEnumerable> (_intArrayExpression)).Return (new List<int>());

      _providerMock.Replay ();
      QueryableBase<int> queryable = new TestQueryable<int> (_providerMock, _intArrayExpression);
      ((IEnumerable)queryable).GetEnumerator ();
      _providerMock.VerifyAllExpectations ();
    }
  }
}
