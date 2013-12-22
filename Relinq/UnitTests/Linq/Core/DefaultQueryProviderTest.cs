// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.Parsing.Structure;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core
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
        + "but was 'Remotion.Linq.UnitTests.Linq.Core.TestDomain.TestQueryable`1[System.Int32]'."
        + "\r\nParameter name: queryableType")]
    public void Initialization_NonGenericTypeDefinition ()
    {
      new DefaultQueryProvider (typeof (TestQueryable<int>), _parserStub, _executorStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Expected the generic type definition of an implementation of IQueryable<T> with exactly one type argument, "
        + "but found 2 arguments on 'Remotion.Linq.UnitTests.Linq.Core.TestDomain.QueryableWithTooManyArguments`2[T1,T2]."
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