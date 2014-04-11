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

namespace Remotion.Linq.Clauses.StreamedData
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
