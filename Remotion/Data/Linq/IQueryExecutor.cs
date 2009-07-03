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
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.EagerFetching;
using System.Collections.Generic;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// Constitutes the bridge between re-linq and a concrete query provider implementation. Concrete providers implement this interface
  /// and <see cref="QueryProviderBase"/> calls the respective method of the interface implementation when a query is to be executed.
  /// </summary>
  public interface IQueryExecutor
  {
    /// <summary>
    /// Executes the given <paramref name="queryModel"/> as a scalar query, i.e. as a query returning a scalar value of type <typeparamref name="T"/>.
    /// The query ends with a scalar result modification, for example a <see cref="CountResultModification"/> or a <see cref="SumResultModification"/>.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value returned by the query.</typeparam>
    /// <param name="queryModel">The <see cref="QueryModel"/> representing the query to be executed. Analyze this via an 
    /// <see cref="IQueryModelVisitor"/>.</param>
    /// <param name="fetchRequests">The fetch requests to be executed together with this query. Fetch requests are used to specify that the query
    /// should (in addition to returning its result set) fetch a number of dependent objects as well. The fetched objects are not returned, but
    /// they can be cached by the implementation of <see cref="IQueryExecutor"/> so that later queries for them can be faster to execute.</param>
    /// <returns>A scalar value of type <typeparamref name="T"/> that represents the query's result.</returns>
    T ExecuteScalar<T> (QueryModel queryModel, IEnumerable<FetchRequestBase> fetchRequests);

    /// <summary>
    /// Executes the given <paramref name="queryModel"/> as a collection query, i.e. as a query returning objects of type <typeparamref name="T"/>. 
    /// The query does not end with a scalar result modification, but it can end with a single result modification, for example 
    /// <see cref="SingleResultModification"/> or <see cref="FirstResultModification"/>. In such a case, the returned enumerable must yield exactly 
    /// one object (or none if the last result modification allows empty result sets).
    /// </summary>
    /// <typeparam name="T">The type of the items returned by the query.</typeparam>
    /// <param name="queryModel">The <see cref="QueryModel"/> representing the query to be executed. Analyze this via an 
    /// <see cref="IQueryModelVisitor"/>.</param>
    /// <param name="fetchRequests">The fetch requests to be executed together with this query. Fetch requests are used to specify that the query
    /// should (in addition to returning its result set) fetch a number of dependent objects as well. The fetched objects are not returned, but
    /// they can be cached by the implementation of <see cref="IQueryExecutor"/> so that later queries for them can be faster to execute.</param>
    /// <returns>A scalar value of type <typeparamref name="T"/> that represents the query's result.</returns>
    IEnumerable<T> ExecuteCollection<T> (QueryModel queryModel, IEnumerable<FetchRequestBase> fetchRequests);
  }
}
