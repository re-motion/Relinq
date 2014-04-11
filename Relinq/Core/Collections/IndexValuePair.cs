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
using Remotion.Utilities;

namespace Remotion.Linq.Collections
{
  /// <summary>
  /// Represents an item enumerated by <see cref="ObservableCollectionExtensions.AsChangeResistantEnumerableWithIndex{T}"/>. This provides access
  /// to the <see cref="Index"/> as well as the <see cref="Value"/> of the enumerated item.
  /// </summary>
  public struct IndexValuePair<T>
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