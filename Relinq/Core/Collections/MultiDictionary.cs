// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Collections
{
  /// <summary>
  /// Provides an implementation of <see cref="IDictionary{TKey,TValue}"/> that allows storing multiple values per key. The multiple values
  /// are represented as an <see cref="IList{T}"/> of value. Access to a key without values returns an empty <see cref="IList{T}"/>.
  /// </summary>
  /// <typeparam name="TKey">The type of the keys of the values to be stored.</typeparam>
  /// <typeparam name="TValue">The type of the values to be stored.</typeparam>
  public class MultiDictionary<TKey, TValue> : IDictionary<TKey, IList<TValue>>
  {
    private readonly Dictionary<TKey, IList<TValue>> _innerDictionary = new Dictionary<TKey, IList<TValue>>();

    public IEnumerator<KeyValuePair<TKey, IList<TValue>>> GetEnumerator ()
    {
      return _innerDictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return GetEnumerator();
    }

    public void Add (TKey key, TValue item)
    {
      ArgumentUtility.CheckNotNull ("item", item);
      this[key].Add (item);
    }

    void ICollection<KeyValuePair<TKey, IList<TValue>>>.Add (KeyValuePair<TKey, IList<TValue>> item)
    {
      ArgumentUtility.CheckNotNull ("item", item);
      Add (item.Key, item.Value);
    }

    public void Add (TKey key, IList<TValue> value)
    {
      ArgumentUtility.CheckNotNull ("key", key);
      ArgumentUtility.CheckNotNull ("value", value);

      _innerDictionary.Add (key, value);
    }

    public void Clear ()
    {
      _innerDictionary.Clear();
    }

    bool ICollection<KeyValuePair<TKey, IList<TValue>>>.Contains (KeyValuePair<TKey, IList<TValue>> item)
    {
      ArgumentUtility.CheckNotNull ("item", item);
      return _innerDictionary.Contains (item);
    }

    public bool ContainsKey (TKey key)
    {
      return _innerDictionary.ContainsKey (key);
    }

    public bool Remove (TKey key)
    {
      ArgumentUtility.CheckNotNull ("key", key);
      return _innerDictionary.Remove (key);
    }

    bool ICollection<KeyValuePair<TKey, IList<TValue>>>.Remove (KeyValuePair<TKey, IList<TValue>> item)
    {
      ArgumentUtility.CheckNotNull ("item", item);
      return ((IDictionary<TKey, IList<TValue>>) _innerDictionary).Remove (item);
    }

    public int KeyCount
    {
      get { return _innerDictionary.Count; }
    }

    public int CountValues ()
    {
      return this.Sum (kvp => kvp.Value.Count);
    }

    int ICollection<KeyValuePair<TKey, IList<TValue>>>.Count
    {
      get { return KeyCount; }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }
    
    public bool TryGetValue (TKey key, out IList<TValue> value)
    {
      ArgumentUtility.CheckNotNull ("key", key);
      return _innerDictionary.TryGetValue (key, out value);
    }

    public IList<TValue> this [TKey key]
    {
      get
      {
        IList<TValue> valueList;
        if (!_innerDictionary.TryGetValue (key, out valueList))
        {
          valueList = new List<TValue>();
          _innerDictionary.Add (key, valueList);
        }
        return valueList;
      }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        _innerDictionary[key] = value;
      }
    }

    public ICollection<TKey> Keys
    {
      get { return _innerDictionary.Keys; }
    }

    public ICollection<IList<TValue>> Values
    {
      get { return _innerDictionary.Values; }
    }

    void ICollection<KeyValuePair<TKey, IList<TValue>>>.CopyTo (KeyValuePair<TKey, IList<TValue>>[] array, int arrayIndex)
    {
      ((IDictionary<TKey, IList<TValue>>) _innerDictionary).CopyTo (array, arrayIndex);
    }

  }
}