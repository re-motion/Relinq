using System.Linq.Expressions;
using Remotion.Data.Linq.QueryProviderImplementation;

namespace Remotion.Data.Linq.UnitTests
{
  public class TestQueryable<T> : QueryableBase<T>
  {
    public TestQueryable (QueryProviderBase provider, Expression expression)
        : base (provider, expression)
    {
    }

    public TestQueryable (IQueryExecutor executor)
        : base (new TestQueryProvider (executor))
    {
    }

    public override string ToString ()
    {
      return "TestQueryable<" + typeof (T).Name + ">()";
    }
  }
}