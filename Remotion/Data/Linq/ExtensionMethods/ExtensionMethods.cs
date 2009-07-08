// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Utilities;

namespace Remotion.Data.Linq.ExtensionMethods
{
  public static class ExtensionMethods
  {
    /// <summary>
    /// Specifies that, when the <paramref name="query"/> is executed, the relation indicated by <paramref name="relatedObjectSelector"/> should be eagerly
    /// fetched if supported by the query provider implementation. The relation must be a collection property.
    /// </summary>
    /// <typeparam name="TOriginating">The type of the originating query result objects.</typeparam>
    /// <typeparam name="TRelated">The type of the related objects to be eager-fetched.</typeparam>
    /// <param name="query">The query for which the fetch request should be made.</param>
    /// <param name="relatedObjectSelector">A lambda expression selecting the related objects to be eager-fetched.</param>
    /// <returns>A <see cref="FluentFetchRequest{TOriginating, TRelated}"/> object on which further fetch requests can be made. The subsequent fetches start from the 
    /// related objects fetched by the original request created by this method.</returns>
    public static FluentFetchRequest<TOriginating, TRelated> FetchMany<TOriginating, TRelated> (this IQueryable<TOriginating> query, Expression<Func<TOriginating, IEnumerable<TRelated>>> relatedObjectSelector)
    {
      return FetchInternal<TOriginating, TRelated> (query, relatedObjectSelector, new FetchManyExpression (query.Expression, (LambdaExpression) relatedObjectSelector));
    }

    /// <summary>
    /// Specifies that, when the <paramref name="query"/> is executed, the relation indicated by <paramref name="relatedObjectSelector"/> should be eagerly
    /// fetched if supported by the query provider implementation. The relation must be of cardinality one.
    /// </summary>
    /// <typeparam name="TOriginating">The type of the originating query result objects.</typeparam>
    /// <typeparam name="TRelated">The type of the related objects to be eager-fetched.</typeparam>
    /// <param name="query">The query for which the fetch request should be made.</param>
    /// <param name="relatedObjectSelector">A lambda expression selecting the related object to be eager-fetched.</param>
    /// <returns>A <see cref="FluentFetchRequest{TOriginating, TRelated}"/> object on which further fetch requests can be made. The subsequent fetches start from the 
    /// related object fetched by the original request created by this method.</returns>
    public static FluentFetchRequest<TOriginating, TRelated> FetchOne<TOriginating, TRelated> (this IQueryable<TOriginating> query, Expression<Func<TOriginating, TRelated>> relatedObjectSelector)
    {
      return FetchInternal<TOriginating, TRelated>(query, relatedObjectSelector, new FetchOneExpression (query.Expression, (LambdaExpression) relatedObjectSelector));
    }

    private static FluentFetchRequest<TOriginating, TRelated> FetchInternal<TOriginating, TRelated> (IQueryable<TOriginating> query, LambdaExpression relatedObjectSelector, FetchExpression fetchExpression)
    {
      var queryProvider = ArgumentUtility.CheckNotNullAndType<QueryProviderBase> ("query.Provider", query.Provider);
      ArgumentUtility.CheckNotNull ("relatedObjectSelector", relatedObjectSelector);

      return new FluentFetchRequest<TOriginating, TRelated> (queryProvider, fetchExpression);
    }
  }
}
