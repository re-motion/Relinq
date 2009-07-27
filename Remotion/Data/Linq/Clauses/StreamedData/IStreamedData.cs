using System;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;

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
    /// Gets the current single value held by this <see cref="IStreamedData"/> object, throwing an exception if the object does not hold a
    /// single value or if value is not of type  <typeparamref name="T"/>. If the object is used as input, this 
    /// holds the input value for the operation. If the object is used as output, this holds the result of the operation.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <returns>The current value, cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the current value is a sequence or not of the expected type.</exception>
    T GetCurrentSingleValue<T> ();

    /// <summary>
    /// Gets the current sequence held by this <see cref="IStreamedData"/> object as well as an <see cref="Expression"/> describing the 
    /// sequence's items, throwing an exception if the object does not hold a sequence of items of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected item type of the sequence.</typeparam>
    /// <returns>The sequence and an <see cref="Expression"/> describing its items.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this object holds a single value instead of a sequence or the item type is not the 
    /// expected type <typeparamref name="T"/>.</exception>
    TypedSequenceInfo<T> GetCurrentSequenceInfo<T> ();

    /// <summary>
    /// Gets the current sequence held by this <see cref="IStreamedData"/> object as well as an <see cref="Expression"/> describing the 
    /// sequence's items, throwing an exception if the object does not hold a sequence of items.
    /// </summary>
    /// <returns>The sequence and an <see cref="Expression"/> describing its items.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this object holds a single value instead of a sequence.</exception>
    UntypedSequenceInfo GetCurrentSequenceInfo ();
  }
}