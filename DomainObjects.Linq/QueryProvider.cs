using System.Linq;
using System.Linq.Expressions;
using Rubicon.Data.Linq;
using System;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
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