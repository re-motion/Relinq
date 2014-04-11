// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Remotion.Utilities;

namespace Remotion.Linq.Collections
{
  /// <summary>
  /// Extension methods for <see cref="ObservableCollection{T}"/>
  /// </summary>
  public static class ObservableCollectionExtensions
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