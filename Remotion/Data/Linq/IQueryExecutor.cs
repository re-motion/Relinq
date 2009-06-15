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
using System.Collections;
using Remotion.Data.Linq.EagerFetching;
using System.Collections.Generic;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// The interface has to be impelemented so that a query can be executed against the used data backend.
  /// </summary>
  public interface IQueryExecutor
  {
    object ExecuteSingle (QueryModel queryModel, IEnumerable<FetchRequestBase> fetchRequests);
    IEnumerable ExecuteCollection (QueryModel queryModel, IEnumerable<FetchRequestBase> fetchRequests);
  }

  ///// <summary>
  ///// Constitutes the bridge between re-linq and a concrete query provider implementation. Concrete providers implement this interface
  ///// to execute the queries, and <see cref="QueryProviderBase"/> calls the respective method of the interface implementation when a query
  ///// is to be executed.
  ///// </summary>
  //public interface IQueryExecutor
  //{
  //  /// <summary>
  //  /// Executes the given <paramref name="queryModel"/> as a scalar query, i.e. as a query returning a scalar value of type T. The query
  //  /// ends with a scalar result modification, for example a <see cref="CountResultModification"/> or a <see cref="SumResultModification"/>.
  //  /// </summary>
  //  T ExecuteScalar<T> (QueryModel queryModel, IEnumerable<FetchRequestBase> fetchRequests);

  //  /// <summary>
  //  /// Executes the given <paramref name="queryModel"/> as a collection query, i.e. as a query returning objects. The query does nit end
  //  /// with a scalar result modification, but it can end with a single result modification, for example <see cref="SingleResultModification"/>
  //  /// or <see cref="FirstResultModification"/>. In such a case, the returned enumerable must yield exactly one object (or none if the last
  //  /// result modification allows empty result sets).
  //  /// </summary>
  //  IEnumerable ExecuteCollection (QueryModel queryModel, IEnumerable<FetchRequestBase> fetchRequests);
  //}
}
