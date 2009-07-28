using System;

namespace Remotion.Data.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Holds the data needed to represent the output or input of a part of a query in memory. This is mainly used for 
  /// <see cref="ResultOperatorBase.ExecuteInMemory"/>. The data held by implementations of this interface can be either a value or a sequence.
  /// </summary>
  public interface IStreamedData
  {
    /// <summary>
    /// Gets an object describing the data held by this <see cref="IStreamedData"/> instance.
    /// </summary>
    /// <value>An <see cref="IStreamedDataInfo"/> object describing the data held by this <see cref="IStreamedData"/> instance.</value>
    IStreamedDataInfo DataInfo { get; }

    /// <summary>
    /// Gets the value held by this <see cref="IStreamedData"/> instance.
    /// </summary>
    /// <value>The value.</value>
    object Value { get; }
  }
}