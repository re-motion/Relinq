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
    public StandardQueryable (IQueryProvider provider, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("provider", provider);
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (!typeof (IEnumerable<T>).IsAssignableFrom (expression.Type))
        throw new ArgumentTypeException ("expression", typeof (IEnumerable<T>), expression.Type);

      Provider = provider;
      Expression = expression;
    }

    public IQueryProvider Provider { get; private set; }
    public Expression Expression { get; private set; }

    public Type ElementType
    {
      get { return typeof (T); }
    }

    public IEnumerator<T> GetEnumerator ()
    {
      return Provider.Execute<IEnumerable<T>> (Expression).GetEnumerator ();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return ((IEnumerable)Provider.Execute (Expression)).GetEnumerator ();
    }
  }
}