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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq
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
    public void Initialize ()
    {
      QueryableBase<int> queryable = new TestQueryable<int> (_providerMock, _intArrayExpression);

      Assert.AreSame (_providerMock, queryable.Provider);
      Assert.AreSame (_intArrayExpression, queryable.Expression);

      Assert.AreEqual (typeof (int), queryable.ElementType);
    }

    [Test]
    public void InitializeWithDefaultConstructor ()
    {
      var executor = _mockRepository.StrictMock<IQueryExecutor>();
      QueryableBase<int> queryable = new TestQueryable<int> (executor);

      Assert.IsNotNull (queryable.Provider);
      Assert.IsNotNull (queryable.Expression);
      Assert.AreEqual (ExpressionType.Constant, queryable.Expression.NodeType);
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