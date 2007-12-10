using System.Linq;
using System.Linq.Expressions;

namespace Rubicon.Data.Linq.UnitTests
{
  public class TestQueryProvider : QueryProviderBase
  {
    public TestQueryProvider (IQueryExecutor executor)
        : base (executor)
    {
    }

    protected override IQueryable<T> CreateQueryable<T> (Expression expression)
    {
      return new TestQueryable<T> (this, expression);
    }
  }
}