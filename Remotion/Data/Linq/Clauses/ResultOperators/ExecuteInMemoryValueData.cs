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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Holds data needed in order to execute a <see cref="ResultOperatorBase"/> in memory via <see cref="ResultOperatorBase.ExecuteInMemory(Remotion.Data.Linq.Clauses.ResultOperators.IExecuteInMemoryData)"/>.
  /// The data is a single, non-sequence value and can only be consumed by result operators working with single values.
  /// </summary>
  public class ExecuteInMemoryValueData : IExecuteInMemoryData
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteInMemoryValueData"/> class, setting the <see cref="CurrentValue"/> property.
    /// </summary>
    /// <param name="currentValue">The current value or sequence.</param>
    public ExecuteInMemoryValueData (object currentValue)
    {
      CurrentValue = currentValue;
    }

    /// <summary>
    /// Gets the current value for the <see cref="ResultOperatorBase.ExecuteInMemory(Remotion.Data.Linq.Clauses.ResultOperators.IExecuteInMemoryData)"/> operation. If the object is used as input, this 
    /// holds the input value for the operation. If the object is used as output, this holds the result of the operation.
    /// </summary>
    /// <value>The current value.</value>
    public object CurrentValue { get; private set; }

    /// <summary>
    /// Gets the current single value held by <see cref="CurrentValue"/>, throwing an exception if the value is not of type 
    /// <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <returns><see cref="CurrentValue"/>, cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="CurrentValue"/> if not of the expected type.</exception>
    public T GetCurrentSingleValue<T> ()
    {
      try
      {
        return (T) CurrentValue;
      }
      catch (InvalidCastException ex)
      {
        string message = string.Format (
            "Cannot retrieve the current value as type '{0}' because it is of type '{1}'.",
            typeof (T).FullName,
            CurrentValue.GetType ().FullName);
        throw new InvalidOperationException (message, ex);
      }
      catch (NullReferenceException ex)
      {
        string message = string.Format ("Cannot retrieve the current value as type '{0}' because it is null.", typeof (T).FullName);
        throw new InvalidOperationException (message, ex);
      }
    }

    /// <summary>
    /// Throws an exception because the value is not a sequence.
    /// </summary>
    /// <typeparam name="T">The expected item type of the sequence.</typeparam>
    /// <exception cref="InvalidOperationException">This object does not hold a sequence.</exception>
    TypedSequenceInfo<T> IExecuteInMemoryData.GetCurrentSequenceInfo<T> ()
    {
      throw new InvalidOperationException ("Cannot retrieve the current value as a sequence because it is a value.");
    }

    /// <summary>
    /// Throws an exception because the value is not a sequence.
    /// </summary>
    /// <exception cref="InvalidOperationException">This object does not hold a sequence.</exception>
    UntypedSequenceInfo IExecuteInMemoryData.GetCurrentSequenceInfo ()
    {
      throw new InvalidOperationException ("Cannot retrieve the current value as a sequence because it is a value.");
    }

    /// <summary>
    /// Takes the given <paramref name="genericMethodDefinition"/> and instantiates it, substituting its generic parameter with the value
    /// type of the value held by this object. The method must have exactly one generic parameter.
    /// </summary>
    /// <param name="genericMethodDefinition">The generic method definition to instantiate.</param>
    /// <returns>
    /// A closed generic instantiation of <paramref name="genericMethodDefinition"/> with this object's value type substituted for
    /// the generic parameter.
    /// </returns>
    public MethodInfo MakeClosedGenericExecuteMethod (MethodInfo genericMethodDefinition)
    {
      ArgumentUtility.CheckNotNull ("genericMethodDefinition", genericMethodDefinition);

      if (!genericMethodDefinition.IsGenericMethodDefinition)
        throw new ArgumentException ("GenericMethodDefinition must be a generic method definition.", "genericMethodDefinition");

      if (genericMethodDefinition.GetGenericArguments ().Length != 1)
        throw new ArgumentException ("GenericMethodDefinition must have exactly one generic parameter.", "genericMethodDefinition");

      if (CurrentValue == null)
        return genericMethodDefinition.MakeGenericMethod (typeof (object));
      else
        return genericMethodDefinition.MakeGenericMethod (CurrentValue.GetType ());
    }
  }
}