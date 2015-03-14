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

namespace Remotion.Linq.Collections
{
  /// <summary>
  /// Represents an item enumerated by <see cref="ObservableCollectionExtensions.AsChangeResistantEnumerableWithIndex{T}"/>. This provides access
  /// to the <see cref="Index"/> as well as the <see cref="Value"/> of the enumerated item.
  /// </summary>
  internal struct IndexValuePair<T>
  {
    private readonly ChangeResistantObservableCollectionEnumerator<T> _enumerator;

    public IndexValuePair (ChangeResistantObservableCollectionEnumerator<T> enumerator)
    {
      ArgumentUtility.CheckNotNull ("enumerator", enumerator);
      _enumerator = enumerator;
    }

    /// <summary>
    /// Gets the index of the current enumerated item. Can only be called while enumerating, afterwards, it will throw an 
    /// <see cref="ObjectDisposedException"/>. If an item is inserted into or removed from the collection before the current item, this
    /// index will change.
    /// </summary>
    public int Index { get { return _enumerator.Index; } }

    /// <summary>
    /// Gets the value of the current enumerated item. Can only be called while enumerating, afterwards, it will throw an 
    /// <see cref="ObjectDisposedException"/>.
    /// </summary>
    /// <value>The value.</value>
    public T Value { get { return _enumerator.Current; } }
  }
}