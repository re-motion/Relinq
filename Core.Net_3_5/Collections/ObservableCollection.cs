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
using System.Collections.ObjectModel;

namespace Remotion.Linq.Collections
{
  /// <summary>
  /// Extends <see cref="Collection{T}"/> with events that indicate when the collection was changed.
  /// </summary>
  /// <typeparam name="T">The type of items held by this <see cref="ObservableCollection{T}"/>.</typeparam>
  public sealed class ObservableCollection<T> : Collection<T>
  {
    /// <summary>
    /// Occurs after an item was changed in this <see cref="ObservableCollection{T}"/>.
    /// </summary>
    public event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;

    protected override void ClearItems ()
    {
      base.ClearItems ();

      if (CollectionChanged != null)
        CollectionChanged (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset, -1, -1, null, null));
    }

    protected override void RemoveItem (int index)
    {
      var oldItem = this[index];

      base.RemoveItem (index);

      if (CollectionChanged != null)
      {
        CollectionChanged (
            this,
            new NotifyCollectionChangedEventArgs (
                NotifyCollectionChangedAction.Remove,
                newStartingIndex:-1,
                oldStartingIndex: index,
                newItems: null,
                oldItems: new[] { oldItem }));
      }
    }

    protected override void InsertItem (int index, T item)
    {
      base.InsertItem (index, item);

      if (CollectionChanged != null)
      {
        CollectionChanged (
            this,
            new NotifyCollectionChangedEventArgs (
                NotifyCollectionChangedAction.Add,
                newStartingIndex: index,
                oldStartingIndex: -1,
                newItems: new[] { item },
                oldItems: null));
      }
    }

    protected override void SetItem (int index, T item)
    {
      var oldItem = this[index];

      base.SetItem (index, item);

      if (CollectionChanged != null)
      {
        CollectionChanged (
            this,
            new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, index, index, new[] { item }, new[] { oldItem }));
      }
    }
  }
}