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
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.QueryProviderImplementation;
using Remotion.Utilities;

namespace Remotion.Data.Linq.ExtensionMethods
{
  public static class ExtensionMethods
  {
    public static bool ContainsFulltext(this string extension, string search)
    {
      throw new NotImplementedException ("This method should not be executed. It should be used only in queries analyzed and parsed by re-linq.");
    }

    /// <summary>
    /// Specifies that, when the <paramref name="query"/> is executed, the relation indicated by <paramref name="relatedObjectSelector"/> should be eagerly
    /// fetched if supported by the query provider implementation.
    /// </summary>
    /// <typeparam name="TOriginating">The type of the originating query result objects.</typeparam>
    /// <typeparam name="TRelated">The type of the related objects to be eager-fetched.</typeparam>
    /// <param name="query">The query for which the fetch request should be made.</param>
    /// <param name="relatedObjectSelector">A lambda expression selecting the related objects to be eager-fetched.</param>
    /// <returns>A <see cref="FluentFetchRequest{T}"/> object on which further fetch requests can be made. The subsequent fetches start from the 
    /// related objects fetched by the original request created by this method.</returns>
    public static FluentFetchRequest<TRelated> Fetch<TOriginating, TRelated> (this IQueryable<TOriginating> query, Expression<Func<TOriginating, IEnumerable<TRelated>>> relatedObjectSelector)
    {
      var queryable = ArgumentUtility.CheckNotNullAndType<QueryableBase<TOriginating>> ("query", query);
      ArgumentUtility.CheckNotNull ("relatedObjectSelector", relatedObjectSelector);

      var request = queryable.GetOrAddFetchRequest (relatedObjectSelector);
      return new FluentFetchRequest<TRelated> (request);
    }
  }
}
