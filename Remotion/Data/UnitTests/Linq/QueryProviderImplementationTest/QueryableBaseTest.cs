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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.QueryProviderImplementation;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.QueryProviderImplementationTest
{
  [TestFixture]
  public class QueryableBaseTest
  {
    private QueryProviderBase _providerMock;
    private MockRepository _mockRepository;
    private Expression _intArrayExpression;
    private Expression _studentArrayExpression;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      var executorMock = _mockRepository.StrictMock<IQueryExecutor>();
      _providerMock = _mockRepository.StrictMock<QueryProviderBase> (executorMock);

      _intArrayExpression = ExpressionHelper.CreateNewIntArrayExpression ();
      _studentArrayExpression = Expression.Constant (new Student[0]);
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
      Expect.Call (_providerMock.ExecuteCollection<int> (_intArrayExpression)).Return (new List<int> ());

      _mockRepository.ReplayAll ();
      QueryableBase<int> queryable = new TestQueryable<int> (_providerMock, _intArrayExpression);
      queryable.GetEnumerator();
      _mockRepository.VerifyAll ();
    }

    [Test]
    public void GetEnumerator()
    {
      Expect.Call (_providerMock.ExecuteCollection (_intArrayExpression)).Return (new List<int> ());

      _mockRepository.ReplayAll ();
      QueryableBase<int> queryable = new TestQueryable<int> (_providerMock, _intArrayExpression);
      ((IEnumerable)queryable).GetEnumerator ();
      _mockRepository.VerifyAll ();
    }

    [Test]
    public void GetOrAddFetchRequest ()
    {
      QueryableBase<Student> queryable = new TestQueryable<Student> (_providerMock, _studentArrayExpression);

      Assert.That (queryable.FetchRequests, Is.Empty);

      Expression<Func<Student, IEnumerable<int>>> expectedExpression = s => s.Scores;
      var result = queryable.GetOrAddFetchRequest (expectedExpression);

      Assert.That (result.RelatedObjectSelector, Is.SameAs (expectedExpression));
      Assert.That (queryable.FetchRequests, Is.EqualTo (new[] { result }));
    }
  }
}
