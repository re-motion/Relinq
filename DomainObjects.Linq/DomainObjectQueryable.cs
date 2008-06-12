using System.Linq.Expressions;
using Remotion.Data.Linq;
using Remotion.Data.Linq.QueryProviderImplementation;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Mixins;


namespace Remotion.Data.DomainObjects.Linq
{
  public class DomainObjectQueryable<T> : QueryableBase<T> 
  {
    public DomainObjectQueryable (QueryProviderBase provider, Expression expression)
        : base (provider, expression)
    {
    }

    //public DomainObjectQueryable (IQueryListener listener)
    //    : base (new QueryProvider (new QueryExecutor<T> (listener)))
    //{
    //}

    public DomainObjectQueryable (IQueryListener listener, SqlGeneratorBase sqlGenerator)
      :base (new QueryProvider(ObjectFactory.Create<QueryExecutor<T>>().With(listener,sqlGenerator)))
    {
    }
    
    public string GetCommandString()
    {
      return null;
    }
  }
}