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
using Remotion.Data.Linq.Clauses.ResultOperators;

namespace Remotion.Data.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Holds the data needed to represent the output or input of a part of a query in memory. This is mainly used for 
  /// <see cref="ResultOperatorBase.ExecuteInMemory"/>.  The data is a single, non-sequence value and can only be consumed by result operators 
  /// working with single values.
  /// </summary>
  public class StreamedValue : IStreamedData
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamedValue"/> class, setting the <see cref="CurrentValue"/> property.
    /// </summary>
    /// <param name="currentValue">The current value or sequence.</param>
    public StreamedValue (object currentValue)
    {
      CurrentValue = currentValue;
      DataInfo = new StreamedValueInfo (currentValue != null ? currentValue.GetType () : typeof (object));
    }

    /// <summary>
    /// Gets the current value for the <see cref="ResultOperatorBase.ExecuteInMemory(IStreamedData)"/> operation. If the object is used as input, this 
    /// holds the input value for the operation. If the object is used as output, this holds the result of the operation.
    /// </summary>
    /// <value>The current value.</value>
    public object CurrentValue { get; private set; }

    /// <summary>
    /// Gets an object describing the data held by this <see cref="StreamedValue"/> instance.
    /// </summary>
    /// <value>
    /// An <see cref="StreamedValueInfo"/> object describing the data held by this <see cref="StreamedValue"/> instance.
    /// </value>
    public StreamedValueInfo DataInfo { get; private set; }

    IStreamedDataInfo IStreamedData.DataInfo 
    { 
      get { return DataInfo; } 
    }

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
    TypedSequenceInfo<T> IStreamedData.GetCurrentSequenceInfo<T> ()
    {
      throw new InvalidOperationException ("Cannot retrieve the current value as a sequence because it is a value.");
    }

    /// <summary>
    /// Throws an exception because the value is not a sequence.
    /// </summary>
    /// <exception cref="InvalidOperationException">This object does not hold a sequence.</exception>
    UntypedSequenceInfo IStreamedData.GetCurrentSequenceInfo ()
    {
      throw new InvalidOperationException ("Cannot retrieve the current value as a sequence because it is a value.");
    }
  }
}