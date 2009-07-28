using System;
using System.Collections.Generic;
using System.Reflection;
using Remotion.Data.Linq.EagerFetching;

namespace Remotion.Data.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Describes the data streamed out of a <see cref="QueryModel"/> or <see cref="ResultOperatorBase"/>.
  /// </summary>
  public interface IStreamedDataInfo
  {
    /// <summary>
    /// Gets the type of the data described by this <see cref="IStreamedDataInfo"/> instance. For a sequence, this is a type implementing 
    /// <see cref="IEnumerable{T}"/>, where <c>T</c> is instantiated with a concrete type. For a single value, this is the value type.
    /// </summary>
    Type DataType { get; }

    /// <summary>
    /// Takes the given <paramref name="genericMethodDefinition"/> and instantiates it, substituting its generic parameter with the value
    /// or item type of the data described by this object. The method must have exactly one generic parameter.
    /// </summary>
    /// <param name="genericMethodDefinition">The generic method definition to instantiate.</param>
    /// <returns>A closed generic instantiation of <paramref name="genericMethodDefinition"/> with this object's value or item type substituted for
    /// the generic parameter.</returns>
    MethodInfo MakeClosedGenericExecuteMethod (MethodInfo genericMethodDefinition);

    /// <summary>
    /// Executes the specified <see cref="QueryModel"/> with the given <see cref="IQueryExecutor"/>, calling either 
    /// <see cref="IQueryExecutor.ExecuteScalar{T}"/> or <see cref="IQueryExecutor.ExecuteCollection{T}"/>, depending on the type of data streamed
    /// from this interface.
    /// </summary>
    /// <param name="queryModel">The query model to be executed.</param>
    /// <param name="fetchRequests">The fetch requests to pass to <paramref name="executor"/>.</param>
    /// <param name="executor">The executor to use.</param>
    /// <returns>An <see cref="IStreamedData"/> object holding the results of the query execution.</returns>
    IStreamedData ExecuteQueryModel (QueryModel queryModel, FetchRequestBase[] fetchRequests, IQueryExecutor executor);
  }
}