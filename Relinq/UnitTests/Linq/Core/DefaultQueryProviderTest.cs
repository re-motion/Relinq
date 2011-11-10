// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Utilities;
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
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_NonGeneric ()
    {
      new DefaultQueryProvider (typeof (int), _parserStub, _executorStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_NonGenericTypeDefinition ()
    {
      new DefaultQueryProvider (typeof (TestQueryable<int>), _parserStub, _executorStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_TooManyGenericArguments ()
    {
      new DefaultQueryProvider (typeof (QueryableWithTooManyArguments<,>), _parserStub, _executorStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
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