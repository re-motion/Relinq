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
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Holds the data needed to represent the output or input of a part of a query in memory. This is mainly used for 
  /// <see cref="ResultOperatorBase.ExecuteInMemory"/>.  The data is a single, non-sequence value and can only be consumed by result operators 
  /// working with single values.
  /// </summary>
  public sealed class StreamedValue : IStreamedData
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamedValue"/> class, setting the <see cref="Value"/> and <see cref="DataInfo"/> properties.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="streamedValueInfo">A <see cref="StreamedValueInfo"/> describing the value.</param>
    public StreamedValue (object value, StreamedValueInfo streamedValueInfo)
    {
      ArgumentUtility.CheckNotNull ("streamedValueInfo", streamedValueInfo);
      ArgumentUtility.CheckType ("value", value, streamedValueInfo.DataType);

      Value = value;
      DataInfo = streamedValueInfo;
    }

    /// <summary>
    /// Gets an object describing the data held by this <see cref="StreamedValue"/> instance.
    /// </summary>
    /// <value>
    /// An <see cref="StreamedValueInfo"/> object describing the data held by this <see cref="StreamedValue"/> instance.
    /// </value>
    public StreamedValueInfo DataInfo { get; private set; }

    /// <summary>
    /// Gets the current value for the <see cref="ResultOperatorBase.ExecuteInMemory(IStreamedData)"/> operation. If the object is used as input, this 
    /// holds the input value for the operation. If the object is used as output, this holds the result of the operation.
    /// </summary>
    /// <value>The current value.</value>
    public object Value { get; private set; }

    IStreamedDataInfo IStreamedData.DataInfo 
    { 
      get { return DataInfo; } 
    }

    /// <summary>
    /// Gets the value held by <see cref="Value"/>, throwing an exception if the value is not of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <returns><see cref="Value"/>, cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Value"/> if not of the expected type.</exception>
    public T GetTypedValue<T> ()
    {
      try
      {
        return (T) Value;
      }
      catch (InvalidCastException ex)
      {
        string message = string.Format (
            "Cannot retrieve the current value as type '{0}' because it is of type '{1}'.",
            typeof (T).FullName,
            Value.GetType ().FullName);
        throw new InvalidOperationException (message, ex);
      }
      catch (NullReferenceException ex)
      {
        string message = string.Format ("Cannot retrieve the current value as type '{0}' because it is null.", typeof (T).FullName);
        throw new InvalidOperationException (message, ex);
      }
    }
  }
}
