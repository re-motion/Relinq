using System.Linq.Expressions;
using Rubicon.Data.Linq.QueryProviderImplementation;

namespace Rubicon.Data.Linq.UnitTests
{
  public class TestQueryable<T> : QueryableBase<T>
  {
    public TestQueryable (QueryProviderBase provider, Expression expression)
        : base (provider, expression)
    {
    }

    public TestQueryable (IQueryExecutor executor)
        : base (executor)
    {
    }

    protected override QueryProviderBase CreateQueryProvider (IQueryExecutor executor)
    {
      return new TestQueryProvider (executor);
    }
  }
}