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
using System.Collections.Generic;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.EagerFetching;

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
    /// The query ends with a scalar result operator, for example a <see cref="CountResultOperator"/> or a <see cref="SumResultOperator"/>.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value returned by the query.</typeparam>
    /// <param name="queryModel">The <see cref="QueryModel"/> representing the query to be executed. Analyze this via an 
    /// <see cref="IQueryModelVisitor"/>.</param>
    /// <param name="fetchRequests">The fetch requests to be executed together with this query. Fetch requests are used to specify that the query
    /// should (in addition to returning its result set) fetch a number of dependent objects as well. The fetched objects are not returned, but
    /// they can be cached by the implementation of <see cref="IQueryExecutor"/> so that later queries for them can be faster to execute.</param>
    /// <returns>A scalar value of type <typeparamref name="T"/> that represents the query's result.</returns>
    /// <remarks>
    /// The difference between <see cref="ExecuteSingle{T}"/> and <see cref="ExecuteScalar{T}"/> is in the kind of object that is returned.
    /// <see cref="ExecuteSingle{T}"/> is used when a query that would otherwise return a collection result set should pick a single value from the 
    /// set, for example the first, last, minimum, maximum, or only value in the set. <see cref="ExecuteScalar{T}"/> is used when a value is 
    /// calculated or aggregated from all the values in the collection result set. This applies to, for example, item counts, average calculations,
    /// checks for the existence of a specific item, and so on.
    /// </remarks>
    T ExecuteScalar<T> (QueryModel queryModel, IEnumerable<FetchRequestBase> fetchRequests);

    /// <summary>
    /// Executes the given <paramref name="queryModel"/> as a single object query, i.e. as a query returning a single object of type 
    /// <typeparamref name="T"/>.
    /// The query ends with a single result operator, for example a <see cref="FirstResultOperator"/> or a <see cref="SingleResultOperator"/>.
    /// </summary>
    /// <typeparam name="T">The type of the single value returned by the query.</typeparam>
    /// <param name="queryModel">The <see cref="QueryModel"/> representing the query to be executed. Analyze this via an 
    /// <see cref="IQueryModelVisitor"/>.</param>
    /// <param name="fetchRequests">The fetch requests to be executed together with this query. Fetch requests are used to specify that the query
    /// should (in addition to returning its result set) fetch a number of dependent objects as well. The fetched objects are not returned, but
    /// they can be cached by the implementation of <see cref="IQueryExecutor"/> so that later queries for them can be faster to execute.</param>
    /// <param name="returnDefaultWhenEmpty">If <see langword="true" />, the executor must return a default value when its result set is empty; 
    /// if <see langword="false" />, it should throw an <see cref="InvalidOperationException"/> when its result set is empty.</param>
    /// <returns>A single value of type <typeparamref name="T"/> that represents the query's result.</returns>
    /// <remarks>
    /// The difference between <see cref="ExecuteSingle{T}"/> and <see cref="ExecuteScalar{T}"/> is in the kind of object that is returned.
    /// <see cref="ExecuteSingle{T}"/> is used when a query that would otherwise return a collection result set should pick a single value from the 
    /// set, for example the first, last, minimum, maximum, or only value in the set. <see cref="ExecuteScalar{T}"/> is used when a value is 
    /// calculated or aggregated from all the values in the collection result set. This applies to, for example, item counts, average calculations,
    /// checks for the existence of a specific item, and so on.
    /// </remarks>
    T ExecuteSingle<T> (QueryModel queryModel, IEnumerable<FetchRequestBase> fetchRequests, bool returnDefaultWhenEmpty);

    /// <summary>
    /// Executes the given <paramref name="queryModel"/> as a collection query, i.e. as a query returning objects of type <typeparamref name="T"/>. 
    /// The query does not end with a scalar result operator, but it can end with a single result operator, for example 
    /// <see cref="SingleResultOperator"/> or <see cref="FirstResultOperator"/>. In such a case, the returned enumerable must yield exactly 
    /// one object (or none if the last result operator allows empty result sets).
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