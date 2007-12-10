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

    public DomainObjectQueryable ()
        : base (null)
    {
    }

    protected override QueryProviderBase CreateQueryProvider (IQueryExecutor executor)
    {
      throw new System.NotImplementedException();
    }
  }
}