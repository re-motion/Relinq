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

    public DomainObjectQueryable (SqlGeneratorBase sqlGenerator)
      : base (new QueryProvider(ObjectFactory.Create<QueryExecutor<T>>().With(sqlGenerator)))
    {
    }
    
  }
}