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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests
{
  [TestFixture]
  public class DefaultQueryProviderTest
  {
    private IQueryParser _parserStub;
    private IQueryExecutor _executorStub;

    [SetUp]
    public void SetUp ()
    {
      _parserStub = MockRepository.GenerateStub<IQueryParser> ();
      _executorStub = MockRepository.GenerateStub<IQueryExecutor> ();
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "Parameter 'queryableType' is a 'System.Int32', which cannot be assigned to type 'System.Linq.IQueryable'.\r\nParameter name: queryableType")]
    public void Initialization_NonGeneric ()
    {
      new DefaultQueryProvider (typeof (int), _parserStub, _executorStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Expected the generic type definition of an implementation of IQueryable<T>, "
        + "but was 'Remotion.Linq.Development.UnitTesting.TestQueryable`1[System.Int32]'."
        + "\r\nParameter name: queryableType")]
    public void Initialization_NonGenericTypeDefinition ()
    {
      new DefaultQueryProvider (typeof (TestQueryable<int>), _parserStub, _executorStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Expected the generic type definition of an implementation of IQueryable<T> with exactly one type argument, "
        + "but found 2 arguments on 'Remotion.Linq.UnitTests.TestDomain.QueryableWithTooManyArguments`2[T1,T2]."
        + "\r\nParameter name: queryableType")]
    public void Initialization_TooManyGenericArguments ()
    {
      new DefaultQueryProvider (typeof (QueryableWithTooManyArguments<,>), _parserStub, _executorStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Parameter 'queryableType' is a 'System.Collections.Generic.List`1[System.Int32]', which cannot be assigned to type 'System.Linq.IQueryable'."
        + "\r\nParameter name: queryableType")]
    public void Initialization_NonQueryable ()
    {
      new DefaultQueryProvider (typeof (List<int>), _parserStub, _executorStub);
    }

    [Test]
    public void CreateQueryable ()
    {
      var provider = new DefaultQueryProvider (typeof (TestQueryable<>), _parserStub, _executorStub);
      var expression = Expression.Constant (new[] {0});

      var queryable = provider.CreateQuery<int> (expression);

      Assert.That (queryable, Is.InstanceOf (typeof (TestQueryable<int>)));
      Assert.That (queryable.Provider, Is.SameAs (provider));
      Assert.That (queryable.Expression, Is.SameAs (expression));
    }
  }
}