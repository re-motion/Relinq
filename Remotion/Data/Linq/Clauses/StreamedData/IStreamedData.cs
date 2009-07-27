using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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
    /// Gets the type of the data described by this <see cref="IStreamedData"/> instance. For a sequence, this is a type implementing 
    /// <see cref="IEnumerable{T}"/>, where <c>T</c> is instantiated with a concrete type. For a single value, this is the value type.
    /// </summary>
    Type DataType { get; }

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

    /// <summary>
    /// Takes the given <paramref name="genericMethodDefinition"/> and instantiates it, substituting its generic parameter with the value
    /// or item type of the value held by this object. The method must have exactly one generic parameter.
    /// </summary>
    /// <param name="genericMethodDefinition">The generic method definition to instantiate.</param>
    /// <returns>A closed generic instantiation of <paramref name="genericMethodDefinition"/> with this object's value or item type substituted for
    /// the generic parameter.</returns>
    MethodInfo MakeClosedGenericExecuteMethod (MethodInfo genericMethodDefinition);
  }
}