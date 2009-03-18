// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Utilities;

namespace Remotion.Data.Linq.QueryProviderImplementation
{
  /// <summary>
  /// Acts as a common base class for <see cref="IQueryable{T}"/> implementations based on re-linq.
  /// </summary>
  /// <typeparam name="T">The result type yielded by this query.</typeparam>
  public abstract class QueryableBase<T> : IOrderedQueryable<T>
  {
    private readonly QueryProviderBase _queryProvider;
    private readonly FetchRequestCollection<T> _fetchRequestCollection = new FetchRequestCollection<T> ();

    protected QueryableBase (QueryProviderBase provider)
    {
      ArgumentUtility.CheckNotNull ("provider", provider);

      _queryProvider = provider;
      Expression = Expression.Constant (this);
    }

    protected QueryableBase (QueryProviderBase provider, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("provider", provider);
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (!typeof (IEnumerable<T>).IsAssignableFrom (expression.Type))
        throw new ArgumentTypeException ("expression", typeof (IEnumerable<T>), expression.Type);

      _queryProvider = provider;
      Expression = expression;
    }

    public Expression Expression { get; private set; }

    public IQueryProvider Provider
    {
      get { return _queryProvider; }
    }

    public Type ElementType
    {
      get { return typeof (T); }
    }

    /// <summary>
    /// Gets the fetch requests that were issued for this <see cref="QueryableBase{T}"/>.
    /// </summary>
    /// <value>The fetch requests added via <see cref="GetOrAddFetchRequest{TRelated}"/>.</value>
    public IEnumerable<IFetchRequest> FetchRequests
    {
      get { return _fetchRequestCollection.FetchRequests; }
    }

    /// <summary>
    /// Gets or adds an eager-fetch request for this <see cref="QueryableBase{T}"/>.
    /// </summary>
    /// <typeparam name="TRelated">The type of related objects to be fetched.</typeparam>
    /// <param name="relatedObjectSelector">A lambda expression selecting related objects for a given query result object.</param>
    /// <returns>An <see cref="IFetchRequest"/> instance representing the fetch request.</returns>
    public FetchRequest<TRelated> GetOrAddFetchRequest<TRelated> (Expression<Func<T, IEnumerable<TRelated>>> relatedObjectSelector)
    {
      ArgumentUtility.CheckNotNull ("relatedObjectSelector", relatedObjectSelector);
      return _fetchRequestCollection.GetOrAddFetchRequest (relatedObjectSelector);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _queryProvider.ExecuteCollection<T> (Expression).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _queryProvider.ExecuteCollection (Expression).GetEnumerator();
    }
  }
}
