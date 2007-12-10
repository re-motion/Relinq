using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.QueryProviderImplementation;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.UnitTests.QueryProviderImplementationTest
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
      IQueryExecutor executor = _mockRepository.CreateMock<IQueryExecutor>();
      _provider = _mockRepository.CreateMock<QueryProviderBase> (executor);
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
      IQueryExecutor executor = _mockRepository.CreateMock<IQueryExecutor>();
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