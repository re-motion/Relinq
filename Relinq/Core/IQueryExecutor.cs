// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using Remotion.Linq.Clauses.ResultOperators;

namespace Remotion.Linq
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
    /// <returns>A scalar value of type <typeparamref name="T"/> that represents the query's result.</returns>
    /// <remarks>
    /// The difference between <see cref="ExecuteSingle{T}"/> and <see cref="ExecuteScalar{T}"/> is in the kind of object that is returned.
    /// <see cref="ExecuteSingle{T}"/> is used when a query that would otherwise return a collection result set should pick a single value from the 
    /// set, for example the first, last, minimum, maximum, or only value in the set. <see cref="ExecuteScalar{T}"/> is used when a value is 
    /// calculated or aggregated from all the values in the collection result set. This applies to, for example, item counts, average calculations,
    /// checks for the existence of a specific item, and so on.
    /// </remarks>
    T ExecuteScalar<T> (QueryModel queryModel);

    /// <summary>
    /// Executes the given <paramref name="queryModel"/> as a single object query, i.e. as a query returning a single object of type 
    /// <typeparamref name="T"/>.
    /// The query ends with a single result operator, for example a <see cref="FirstResultOperator"/> or a <see cref="SingleResultOperator"/>.
    /// </summary>
    /// <typeparam name="T">The type of the single value returned by the query.</typeparam>
    /// <param name="queryModel">The <see cref="QueryModel"/> representing the query to be executed. Analyze this via an 
    /// <see cref="IQueryModelVisitor"/>.</param>
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
    T ExecuteSingle<T> (QueryModel queryModel, bool returnDefaultWhenEmpty);

    /// <summary>
    /// Executes the given <paramref name="queryModel"/> as a collection query, i.e. as a query returning objects of type <typeparamref name="T"/>. 
    /// The query does not end with a scalar result operator, but it can end with a single result operator, for example 
    /// <see cref="SingleResultOperator"/> or <see cref="FirstResultOperator"/>. In such a case, the returned enumerable must yield exactly 
    /// one object (or none if the last result operator allows empty result sets).
    /// </summary>
    /// <typeparam name="T">The type of the items returned by the query.</typeparam>
    /// <param name="queryModel">The <see cref="QueryModel"/> representing the query to be executed. Analyze this via an 
    /// <see cref="IQueryModelVisitor"/>.</param>
    /// <returns>A scalar value of type <typeparamref name="T"/> that represents the query's result.</returns>
    IEnumerable<T> ExecuteCollection<T> (QueryModel queryModel);
  }
}
