// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.QueryProviderImplementation;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.QueryProviderImplementationTest
{
  [TestFixture]
  public class QueryableBaseTest
  {
    private QueryProviderBase _provider;
    private MockRepository _mockRepository;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      IQueryExecutor executor = _mockRepository.StrictMock<IQueryExecutor>();
      _provider = _mockRepository.StrictMock<QueryProviderBase> (executor);
    }

    [Test]
    public void Initialize ()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression();
      QueryableBase<int> queryable = new TestQueryable<int> (_provider, expression);

      Assert.AreSame (_provider, queryable.Provider);
      Assert.AreSame (expression, queryable.Expression);

      Assert.AreEqual (typeof (int), queryable.ElementType);
    }

    [Test]
    public void InitializeWithDefaultConstructor ()
    {
      IQueryExecutor executor = _mockRepository.StrictMock<IQueryExecutor>();
      QueryableBase<int> queryable = new TestQueryable<int> (executor);

      Assert.IsNotNull (queryable.Provider);
      Assert.IsNotNull (queryable.Expression);
      Assert.AreEqual (ExpressionType.Constant, queryable.Expression.NodeType);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void ConstructorThrowsTypeException ()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression();
      new TestQueryable<string> (_provider, expression);
    }

    [Test]
    public void GenericGetEnumerator ()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression();
      Expect.Call (_provider.ExecuteCollection<int> (expression)).Return (new List<int> ());

      _mockRepository.ReplayAll ();
      QueryableBase<int> queryable = new TestQueryable<int> (_provider, expression);
      queryable.GetEnumerator();
      _mockRepository.VerifyAll ();
    }

    [Test]
    public void GetEnumerator()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression ();
      Expect.Call (_provider.ExecuteCollection (expression)).Return(new List<int>());

      _mockRepository.ReplayAll ();
      QueryableBase<int> queryable = new TestQueryable<int> (_provider, expression);
      ((IEnumerable)queryable).GetEnumerator ();
      _mockRepository.VerifyAll ();
    }
  }
}
