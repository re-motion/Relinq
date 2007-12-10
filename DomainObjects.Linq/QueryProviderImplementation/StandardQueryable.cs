using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.QueryProviderImplementation
{
  public class StandardQueryable<T> : IOrderedQueryable<T>
  {
    private readonly QueryProvider _queryProvider;
    
    public StandardQueryable (IQueryExecutor executor)
    {
      _queryProvider = new QueryProvider (executor);
      Expression = Expression.Constant (this);
    }

    public StandardQueryable (QueryProvider provider, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("provider", provider);
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (!typeof (IEnumerable<T>).IsAssignableFrom (expression.Type))
        throw new ArgumentTypeException ("expression", typeof (IEnumerable<T>), expression.Type);

      _queryProvider = provider;
      Expression = expression;
    }

    public Expression Expression { get; private set; }

    public IQueryProvider Provider { get
    {
      return _queryProvider;
    }
    }

    public Type ElementType
    {
      get { return typeof (T); }
    }

    public IEnumerator<T> GetEnumerator ()
    {
      return _queryProvider.ExecuteCollection<IEnumerable<T>> (Expression).GetEnumerator ();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return _queryProvider.ExecuteCollection (Expression).GetEnumerator ();
    }
  }
}