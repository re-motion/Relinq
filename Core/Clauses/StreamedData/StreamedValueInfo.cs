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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Describes a single or scalar value streamed out of a <see cref="QueryModel"/> or <see cref="ResultOperatorBase"/>.
  /// </summary>
  public abstract class StreamedValueInfo : IStreamedDataInfo
  {
    internal StreamedValueInfo (Type dataType)
    {
      ArgumentUtility.CheckNotNull ("dataType", dataType);
      DataType = dataType;
    }

    /// <summary>
    /// Gets the type of the data described by this <see cref="IStreamedDataInfo"/> instance. This is the type of the streamed value, or 
    /// <see cref="object"/> if the value is <see langword="null" />.
    /// </summary>
    public Type DataType { get; private set; }

    /// <inheritdoc />
    public abstract IStreamedData ExecuteQueryModel (QueryModel queryModel, IQueryExecutor executor);
    
    /// <summary>
    /// Returns a new instance of the same <see cref="StreamedValueInfo"/> type with a different <see cref="DataType"/>.
    /// </summary>
    /// <param name="dataType">The new data type.</param>
    /// <exception cref="ArgumentException">The <paramref name="dataType"/> cannot be used for the clone.</exception>
    /// <returns>A new instance of the same <see cref="StreamedValueInfo"/> type with the given <paramref name="dataType"/>.</returns>
    protected abstract StreamedValueInfo CloneWithNewDataType (Type dataType);

    /// <inheritdoc />
    public virtual IStreamedDataInfo AdjustDataType (Type dataType)
    {
      ArgumentUtility.CheckNotNull ("dataType", dataType);

      if (!dataType.GetTypeInfo().IsAssignableFrom (DataType.GetTypeInfo()))
      {
        var message = string.Format ("'{0}' cannot be used as the new data type for a value of type '{1}'.", dataType, DataType);
        throw new ArgumentException (message, "dataType");
      }

      return CloneWithNewDataType (dataType);
    }

    public override sealed bool Equals (object obj)
    {
      return Equals (obj as IStreamedDataInfo);
    }

    public virtual bool Equals (IStreamedDataInfo obj)
    {
      if (obj == null)
        return false;
    
      if (GetType () != obj.GetType ())
        return false;

      var other = (StreamedValueInfo) obj;
      return DataType.Equals (other.DataType);
    }

    public override int GetHashCode ()
    {
      return DataType.GetHashCode();
    }
  }
}
