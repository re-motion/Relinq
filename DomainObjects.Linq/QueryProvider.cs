using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq;
using Remotion.Utilities;
using System;

namespace Remotion.Data.DomainObjects.Linq
{
  public class QueryProvider : QueryProviderBase
  {
    public QueryProvider (IQueryExecutor executor)
        : base (executor)
    {
    }

    protected override IQueryable<T> CreateQueryable<T> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Type queryableType = typeof (DomainObjectQueryable<>).MakeGenericType (typeof (T));
      return (IQueryable<T>) Activator.CreateInstance (queryableType, this, expression);
    }
  }
}