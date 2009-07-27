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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Holds data needed in order to execute a <see cref="ResultOperatorBase"/> in memory via <see cref="ResultOperatorBase.ExecuteInMemory(IStreamedData)"/>. The
  /// data consists of a sequence of items.
  /// </summary>
  public class StreamedSequence : IStreamedData
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamedSequence"/> class, setting the <see cref="CurrentSequence"/> and 
    /// <see cref="ItemExpression"/> properties.
    /// </summary>
    /// <param name="currentSequence">The current sequence.</param>
    /// <param name="itemExpression">The item expression describing <paramref name="currentSequence"/>'s items.</param>
    public StreamedSequence (IEnumerable currentSequence, Expression itemExpression)
    {
      ArgumentUtility.CheckNotNull ("currentSequence", currentSequence);
      ArgumentUtility.CheckNotNull ("itemExpression", itemExpression);

      var resultItemType = ReflectionUtility.TryGetItemTypeOfIEnumerable (currentSequence.GetType ());
      if (itemExpression.Type != resultItemType)
      {
        var message = string.Format ("ItemExpression is of type {0} but should be {1}.", itemExpression.Type, resultItemType);
        throw new ArgumentTypeException (message, "itemExpression", resultItemType, itemExpression.Type);
      }

      CurrentSequence = currentSequence;
      ItemExpression = itemExpression;
    }

    /// <summary>
    /// Gets the current sequence for the <see cref="ResultOperatorBase.ExecuteInMemory(IStreamedData)"/> operation. If the object is used as input, this 
    /// holds the input sequence for the operation. If the object is used as output, this holds the result of the operation.
    /// </summary>
    /// <value>The current sequence.</value>
    public IEnumerable CurrentSequence { get; private set; }

    /// <summary>
    /// Gets an expression that describes the items held by <see cref="CurrentSequence"/>.
    /// </summary>
    /// <value>The expression for current sequence's items.</value>
    public Expression ItemExpression { get; private set; }

    /// <summary>
    /// Gets the type of the data described by this <see cref="IStreamedData"/> instance. This is a type implementing
    /// <see cref="IEnumerable{T}"/>, where <c>T</c> is instantiated with a concrete type.
    /// </summary>
    public Type DataType
    {
      get { return CurrentSequence.GetType (); }
    }

    /// <summary>
    /// Always throws an exception because this object does not hold a single value, but a sequence.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <exception cref="InvalidOperationException">This object does not hold a single value.</exception>
    T IStreamedData.GetCurrentSingleValue<T> ()
    {
        string message = string.Format (
            "Cannot retrieve the current single value because the current value is a sequence of type '{0}'.",
            CurrentSequence.GetType());
        throw new InvalidOperationException (message);
    }

    /// <summary>
    /// Gets the current sequence held by this object as well as an <see cref="Expression"/> describing the
    /// sequence's items, throwing an exception if the object does not hold a sequence of items of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected item type of the sequence.</typeparam>
    /// <returns>
    /// The sequence and an <see cref="Expression"/> describing its items.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the item type is not the expected type <typeparamref name="T"/>.</exception>
    public TypedSequenceInfo<T> GetCurrentSequenceInfo<T> ()
    {
      try
      {
        return new TypedSequenceInfo<T> ((IEnumerable<T>) CurrentSequence, ItemExpression);
      }
      catch (InvalidCastException ex)
      {
        string message = string.Format (
            "Cannot retrieve the current value as a sequence with item type '{0}' because its items are of type '{1}'.",
            typeof (T).FullName,
            ItemExpression.Type.FullName);

        throw new InvalidOperationException (message, ex);
      }
    }

    /// <summary>
    /// Gets the current sequence held by this object as well as an <see cref="Expression"/> describing the
    /// sequence's items.
    /// </summary>
    /// <returns>
    /// The sequence and an <see cref="Expression"/> describing its items.
    /// </returns>
    public UntypedSequenceInfo GetCurrentSequenceInfo ()
    {
      return new UntypedSequenceInfo (CurrentSequence, ItemExpression);
    }

    /// <summary>
    /// Takes the given <paramref name="genericMethodDefinition"/> and instantiates it, substituting its generic parameter with the 
    /// item type of the value held by this object. The method must have exactly one generic parameter.
    /// </summary>
    /// <param name="genericMethodDefinition">The generic method definition to instantiate.</param>
    /// <returns>
    /// A closed generic instantiation of <paramref name="genericMethodDefinition"/> with this object's item type substituted for
    /// the generic parameter.
    /// </returns>
    public MethodInfo MakeClosedGenericExecuteMethod (MethodInfo genericMethodDefinition)
    {
      ArgumentUtility.CheckNotNull ("genericMethodDefinition", genericMethodDefinition);

      if (!genericMethodDefinition.IsGenericMethodDefinition)
        throw new ArgumentException ("GenericMethodDefinition must be a generic method definition.", "genericMethodDefinition");

      if (genericMethodDefinition.GetGenericArguments ().Length != 1)
        throw new ArgumentException ("GenericMethodDefinition must have exactly one generic parameter.", "genericMethodDefinition");

      return genericMethodDefinition.MakeGenericMethod (ItemExpression.Type);
    }
  }
}