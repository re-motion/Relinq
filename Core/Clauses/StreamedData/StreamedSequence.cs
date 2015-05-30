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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Holds the data needed to represent the output or input of a part of a query in memory. This is mainly used for 
  /// <see cref="ResultOperatorBase.ExecuteInMemory"/>.  The data consists of a sequence of items.
  /// </summary>
  public sealed class StreamedSequence : IStreamedData
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamedSequence"/> class, setting the <see cref="Sequence"/> and 
    /// <see cref="DataInfo"/> properties.
    /// </summary>
    /// <param name="sequence">The sequence.</param>
    /// <param name="streamedSequenceInfo">An instance of <see cref="StreamedSequenceInfo"/> describing the sequence.</param>
    public StreamedSequence ([NotNull] IEnumerable sequence, [NotNull] StreamedSequenceInfo streamedSequenceInfo)
    {
      ArgumentUtility.CheckNotNull ("streamedSequenceInfo", streamedSequenceInfo);
      ArgumentUtility.CheckNotNullAndType ("sequence", sequence, streamedSequenceInfo.DataType);

      DataInfo = streamedSequenceInfo;
      Sequence = sequence;
    }

    [NotNull] 
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
    [NotNull] 
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
    [NotNull] 
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
            DataInfo.ResultItemType.FullName);

        throw new InvalidOperationException (message, ex);
      }
    }
  }
}
