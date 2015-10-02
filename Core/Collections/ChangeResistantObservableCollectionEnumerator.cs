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
using System.Collections.Specialized;
#endif
using Remotion.Utilities;

namespace Remotion.Linq.Collections
{
  /// <summary>
  /// Provides a way to enumerate an <see cref="ObservableCollection{T}"/> while items are inserted, removed, or cleared in a consistent fashion.
  /// </summary>
  /// <typeparam name="T">The element type of the <see cref="ObservableCollection{T}"/>.</typeparam>
  /// <remarks>
  /// This class subscribes to the <see cref="ObservableCollection{T}.CollectionChanged"/> event exposed by <see cref="ObservableCollection{T}"/> 
  /// and reacts on changes to the collection. If an item is inserted or removed before the current element, the enumerator will continue after 
  /// the current element without regarding the new or removed item. If the current item is removed, the enumerator will continue with the item that 
  /// previously followed the current item. If an item is inserted or removed after the current element, the enumerator will simply continue, 
  /// including the newly inserted item and not including the removed item. If an item is moved or replaced, the enumeration will also continue 
  /// with the item located at the next position in the sequence.
  /// </remarks>
  internal class ChangeResistantObservableCollectionEnumerator<T> : IEnumerator<T>
  {
    private readonly ObservableCollection<T> _collection;
    private int _index;
    private bool _disposed;

    public ChangeResistantObservableCollectionEnumerator (ObservableCollection<T> collection)
    {
      ArgumentUtility.CheckNotNull ("collection", collection);

      _collection = collection;
      _index = -1;
      _disposed = false;
      _collection.CollectionChanged += Collection_CollectionChanged;
    }

    public int Index
    {
      get 
      {
        if (_disposed)
          throw new ObjectDisposedException ("enumerator");

        return _index; 
      }
    }

    public void Dispose ()
    {
      if (!_disposed)
      {
        _disposed = true;
        _collection.CollectionChanged -= Collection_CollectionChanged;
      }
    }

    public bool MoveNext ()
    {
      if (_disposed)
        throw new ObjectDisposedException ("enumerator");

      ++_index;
      return _index < _collection.Count;
    }

    public void Reset ()
    {
      if (_disposed)
        throw new ObjectDisposedException ("enumerator");

      _index = -1;
    }

    public T Current
    {
      get
      {
        if (_disposed)
          throw new ObjectDisposedException ("enumerator");
        if (_index < 0)
          throw new InvalidOperationException ("MoveNext must be called before invoking Current.");
        if (_index >= _collection.Count)
          throw new InvalidOperationException ("After MoveNext returned false, Current must not be invoked any more.");

        return _collection[_index];
      }
    }

    object IEnumerator.Current
    {
      get { return Current; }
    }

    private void Collection_CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
    {
      ArgumentUtility.CheckNotNull ("e", e);

      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          if (e.NewStartingIndex <= _index)
            _index += e.NewItems.Count;
          break;
        case NotifyCollectionChangedAction.Remove:
          if (e.OldStartingIndex <= _index)
            _index -= e.OldItems.Count;
          break;
        case NotifyCollectionChangedAction.Replace:
          // NOP
          break;
#if !NET_3_5
        case NotifyCollectionChangedAction.Move:
          // NOP
          break;
#endif
        case NotifyCollectionChangedAction.Reset:
          _index = 0;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
