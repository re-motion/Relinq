// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests
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
    public void Initialize_WithExecutor ()
    {
      var executor = _mockRepository.StrictMock<IQueryExecutor> ();
      QueryableBase<int> queryable = new TestQueryable<int> (executor);

      Assert.That (queryable.Provider, Is.InstanceOfType (typeof (DefaultQueryProvider)));
      Assert.That (((DefaultQueryProvider) queryable.Provider).Executor, Is.SameAs (executor));
      Assert.That (((DefaultQueryProvider) queryable.Provider).QueryableType, Is.SameAs (typeof (TestQueryable<>)));
      Assert.That (queryable.Expression, Is.Not.Null);
      Assert.That (queryable.Expression.NodeType, Is.EqualTo (ExpressionType.Constant));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void ConstructorThrowsTypeException ()
    {
      new TestQueryable<string> (_providerMock, _intArrayExpression);
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
      _providerMock.Expect (mock => mock.Execute (_intArrayExpression)).Return (new List<int>());

      _providerMock.Replay ();
      QueryableBase<int> queryable = new TestQueryable<int> (_providerMock, _intArrayExpression);
      ((IEnumerable)queryable).GetEnumerator ();
      _providerMock.VerifyAllExpectations ();
    }
  }
}
