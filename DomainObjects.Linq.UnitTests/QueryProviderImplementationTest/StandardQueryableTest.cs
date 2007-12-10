using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.QueryProviderImplementation;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.QueryProviderImplementationTest
{
  [TestFixture]
  public class StandardQueryableTest
  {
    private QueryProvider _provider;
    private MockRepository _mockRepository;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      IQueryExecutor executor = _mockRepository.CreateMock<IQueryExecutor>();
      _provider = _mockRepository.CreateMock<QueryProvider> (executor);
    }

    [Test]
    public void Initialize ()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression();
      StandardQueryable<int> queryable = new StandardQueryable<int> (_provider, expression);

      Assert.AreSame (_provider, queryable.Provider);
      Assert.AreSame (expression, queryable.Expression);

      Assert.AreEqual (typeof (int), queryable.ElementType);
    }

    [Test]
    public void InitializeWithDefaultConstructor ()
    {
      IQueryExecutor executor = _mockRepository.CreateMock<IQueryExecutor>();
      StandardQueryable<int> queryable = new StandardQueryable<int> (executor);

      Assert.IsNotNull (queryable.Provider);
      Assert.IsNotNull (queryable.Expression);
      Assert.AreEqual (ExpressionType.Constant, queryable.Expression.NodeType);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void ConstructorThrowsTypeException ()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression();
      new StandardQueryable<string> (_provider, expression);
    }

    [Test]
    public void GenericGetEnumerator ()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression();
      Expect.Call (_provider.ExecuteCollection<IEnumerable<int>> (expression)).Return (new List<int> ());

      _mockRepository.ReplayAll ();
      StandardQueryable<int> queryable = new StandardQueryable<int> (_provider, expression);
      queryable.GetEnumerator();
      _mockRepository.VerifyAll ();
    }

    [Test]
    public void GetEnumerator()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression ();
      Expect.Call (_provider.ExecuteCollection (expression)).Return(new List<int>());

      _mockRepository.ReplayAll ();
      StandardQueryable<int> queryable = new StandardQueryable<int> (_provider, expression);
      ((IEnumerable)queryable).GetEnumerator ();
      _mockRepository.VerifyAll ();
    }
  }
}