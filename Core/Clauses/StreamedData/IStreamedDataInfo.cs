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

namespace Remotion.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Describes the data streamed out of a <see cref="QueryModel"/> or <see cref="ResultOperatorBase"/>.
  /// </summary>
  public interface IStreamedDataInfo : IEquatable<IStreamedDataInfo>
  {
    /// <summary>
    /// Gets the type of the data described by this <see cref="IStreamedDataInfo"/> instance. For a sequence, this is a type implementing 
    /// <see cref="IEnumerable{T}"/>, where <c>T</c> is instantiated with a concrete type. For a single value, this is the value type.
    /// </summary>
    Type DataType { get; }

    /// <summary>
    /// Executes the specified <see cref="QueryModel"/> with the given <see cref="IQueryExecutor"/>, calling either 
    /// <see cref="IQueryExecutor.ExecuteScalar{T}"/> or <see cref="IQueryExecutor.ExecuteCollection{T}"/>, depending on the type of data streamed
    /// from this interface.
    /// </summary>
    /// <param name="queryModel">The query model to be executed.</param>
    /// <param name="executor">The executor to use.</param>
    /// <returns>An <see cref="IStreamedData"/> object holding the results of the query execution.</returns>
    IStreamedData ExecuteQueryModel (QueryModel queryModel, IQueryExecutor executor);

    /// <summary>
    /// Returns a new <see cref="IStreamedDataInfo"/> of the same type as this instance, but with a new <see cref="DataType"/>.
    /// </summary>
    /// <param name="dataType">The type to use for the <see cref="DataType"/> property. The type must be compatible with the data described by this 
    /// <see cref="IStreamedDataInfo"/>, otherwise an exception is thrown.
    /// The type may be a generic type definition if the <see cref="IStreamedDataInfo"/> supports generic types; in this case,
    /// the type definition is automatically closed with generic parameters to match the data described by this <see cref="IStreamedDataInfo"/>.</param>
    /// <returns>A new <see cref="IStreamedDataInfo"/> of the same type as this instance, but with a new <see cref="DataType"/>.</returns>
    /// <exception cref="ArgumentException">The <paramref name="dataType"/> is not compatible with the data described by this 
    /// <see cref="IStreamedDataInfo"/>.</exception>
    IStreamedDataInfo AdjustDataType (Type dataType);
  }
}
