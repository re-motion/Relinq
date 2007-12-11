using System.Linq.Expressions;
using Rubicon.Data.Linq;
using Rubicon.Data.Linq.QueryProviderImplementation;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class DomainObjectQueryable<T> : QueryableBase<T> 
    where T:DomainObject
  {
    public DomainObjectQueryable (QueryProviderBase provider, Expression expression)
        : base (provider, expression)
    {
    }

    public DomainObjectQueryable (IQueryListener listener)
        : base (new QueryExecutor<T> (listener))
    {
    }

    protected override QueryProviderBase CreateQueryProvider (IQueryExecutor executor)
    {
      return new QueryProvider (executor);
    }

    public string GetCommandString()
    {
      return null;
    }
  }
}