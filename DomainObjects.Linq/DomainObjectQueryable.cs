using System.Linq.Expressions;
using Remotion.Data.Linq;
using Remotion.Data.Linq.QueryProviderImplementation;


namespace Remotion.Data.DomainObjects.Linq
{
  public class DomainObjectQueryable<T> : QueryableBase<T> 
  {
    public DomainObjectQueryable (QueryProviderBase provider, Expression expression)
        : base (provider, expression)
    {
    }

    public DomainObjectQueryable (IQueryListener listener)
        : base (new QueryProvider (new QueryExecutor<T> (listener)))
    {
    }

    public string GetCommandString()
    {
      return null;
    }
  }
}