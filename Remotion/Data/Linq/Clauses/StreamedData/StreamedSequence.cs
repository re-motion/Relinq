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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Holds the data needed to represent the output or input of a part of a query in memory. This is mainly used for 
  /// <see cref="ResultOperatorBase.ExecuteInMemory"/>.  The data consists of a sequence of items.
  /// </summary>
  public class StreamedSequence : IStreamedData
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamedSequence"/> class, setting the <see cref="Sequence"/> and 
    /// <see cref="DataInfo"/> properties.
    /// </summary>
    /// <param name="currentSequence">The current sequence.</param>
    /// <param name="itemExpression">The item expression describing <paramref name="currentSequence"/>'s items.</param>
    public StreamedSequence (IEnumerable currentSequence, Expression itemExpression)
    {
      ArgumentUtility.CheckNotNull ("currentSequence", currentSequence);
      ArgumentUtility.CheckNotNull ("itemExpression", itemExpression);

      DataInfo = new StreamedSequenceInfo (currentSequence.GetType (), itemExpression);
      Sequence = currentSequence;
    }

    public StreamedSequenceInfo DataInfo { get; private set; }

    object IStreamedData.Value
    {
      get { return Sequence; }
    }

    IStreamedDataInfo IStreamedData.DataInfo
    {
      get { return DataInfo; }
    }

    /// <summary>
    /// Gets the current sequence for the <see cref="ResultOperatorBase.ExecuteInMemory(IStreamedData)"/> operation. If the object is used as input, this 
    /// holds the input sequence for the operation. If the object is used as output, this holds the result of the operation.
    /// </summary>
    /// <value>The current sequence.</value>
    public IEnumerable Sequence { get; private set; }

    /// <summary>
    /// Gets the current sequence held by this object as well as an <see cref="Expression"/> describing the
    /// sequence's items, throwing an exception if the object does not hold a sequence of items of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected item type of the sequence.</typeparam>
    /// <returns>
    /// The sequence and an <see cref="Expression"/> describing its items.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the item type is not the expected type <typeparamref name="T"/>.</exception>
    public IEnumerable<T> GetTypedSequence<T> ()
    {
      try
      {
        return (IEnumerable<T>) Sequence;
      }
      catch (InvalidCastException ex)
      {
        string message = string.Format (
            "Cannot retrieve the current value as a sequence with item type '{0}' because its items are of type '{1}'.",
            typeof (T).FullName,
            DataInfo.ItemExpression.Type.FullName);

        throw new InvalidOperationException (message, ex);
      }
    }
  }
}