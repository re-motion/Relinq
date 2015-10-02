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
#if !NET_3_5
using System.Collections.ObjectModel;
#endif
using Remotion.Utilities;

namespace Remotion.Linq.Collections
{
  /// <summary>
  /// Extension methods for <see cref="ObservableCollection{T}"/>
  /// </summary>
  internal static class ObservableCollectionExtensions
  {
    private class ChangeResistantEnumerable<T> : IEnumerable<T>
    {
      private readonly ObservableCollection<T> _collection;

      public ChangeResistantEnumerable (ObservableCollection<T> collection)
      {
        ArgumentUtility.CheckNotNull ("collection", collection);
        _collection = collection;
      }

      public IEnumerator<T> GetEnumerator ()
      {
        return new ChangeResistantObservableCollectionEnumerator<T> (_collection);
      }

      IEnumerator IEnumerable.GetEnumerator ()
      {
        return GetEnumerator ();
      }
    }

    /// <summary>
    /// Returns an instance of <see cref="IEnumerable{T}"/> that represents this collection and can be enumerated even while the collection changes;
    /// the enumerator will adapt to the changes (see <see cref="ChangeResistantObservableCollectionEnumerator{T}"/>).
    /// </summary>
    public static IEnumerable<T> AsChangeResistantEnumerable<T> (this ObservableCollection<T> collection)
    {
      return new ChangeResistantEnumerable<T> (collection);
    }

    /// <summary>
    /// Returns an instance of <see cref="IEnumerable{T}"/> that represents this collection and can be enumerated even while the collection changes;
    /// the enumerator will adapt to the changes (see <see cref="ChangeResistantObservableCollectionEnumerator{T}"/>). The enumerable will yield
    /// instances of type <see cref="IndexValuePair{T}"/>, which hold both the index and the value of the current item. If this collection changes
    /// while enumerating, <see cref="IndexValuePair{T}.Index"/> will reflect those changes.
    /// </summary>
    public static IEnumerable<IndexValuePair<T>> AsChangeResistantEnumerableWithIndex<T> (this ObservableCollection<T> collection)
    {
      using (var enumerator = (ChangeResistantObservableCollectionEnumerator<T>) collection.AsChangeResistantEnumerable().GetEnumerator ())
      {
        while (enumerator.MoveNext ())
          yield return new IndexValuePair<T> (enumerator);
      }
    }
  }
}