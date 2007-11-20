using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class QueryProviderTest
  {
    private MockRepository _mockRepository;
    private QueryProvider _queryProvider;
    private IQueryExecutor _executor;

    [SetUp]
    public void SetUp()
    {
      _mockRepository = new MockRepository();
      _executor = _mockRepository.CreateMock<IQueryExecutor>();
      _queryProvider = new QueryProvider (_executor);
    }

    [Test]
    public void Initialization()
    {
      Assert.IsNotNull (_queryProvider);
    }

    [Test]
    public void CreateQueryReturnsIQueryable()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      IQueryable queryable = _queryProvider.CreateQuery (expression);

      Assert.IsNotNull (queryable);
      Assert.IsInstanceOfType (typeof (IQueryable<int>), queryable);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void CreateQueryUnwrapsException ()
    {
      Expression expression = ExpressionHelper.CreateLambdaExpression ();
      _queryProvider.CreateQuery (expression);
    }

    [Test]
    public void GenericCreateQueryReturnsIQueryable ()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression ();
      IQueryable<int> queryable = _queryProvider.CreateQuery<int> (expression);

      Assert.IsNotNull (queryable);
    }

    [Test]
    public void GenericExecute()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression();
      Expect.Call (_executor.Execute<int> (expression)).Return (0);

      _mockRepository.ReplayAll();

      _queryProvider.Execute<int> (expression);

      _mockRepository.VerifyAll();
    }

    [Test]
    public void Execute ()
    {
      Expression expression = ExpressionHelper.CreateNewIntArrayExpression ();
      Expect.Call (_executor.Execute (expression)).Return (0);

      _mockRepository.ReplayAll ();

      _queryProvider.Execute (expression);

      _mockRepository.VerifyAll ();
    }
  }
}