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
using System.Collections.Generic;
using System.Linq;
using Remotion.Utilities;

namespace Remotion.Linq.Collections
{
  /// <summary>
  /// Defines extension methods that simplify working with a dictionary that has a collection-values item-type.
  /// </summary>
  public static class MultiDictionaryExtensions
  {
    public static void Add<TKey, TValue> (this IDictionary<TKey, ICollection<TValue>> dictionary, TKey key, TValue item)
    {
      ArgumentUtility.CheckNotNull ("dictionary", dictionary);
      ArgumentUtility.CheckNotNull ("key", key);
      ArgumentUtility.CheckNotNull ("item", item);

      ICollection<TValue> value;
      if (!dictionary.TryGetValue (key, out value))
      {
        value = new List<TValue>();
        dictionary.Add (key, value);
      }

      value.Add (item);
    }

    public static int CountValues<TKey, TValue> (this IDictionary<TKey, ICollection<TValue>> dictionary)
    {
      ArgumentUtility.CheckNotNull ("dictionary", dictionary);
      
      return dictionary.Sum (kvp => kvp.Value.Count);
    }
  }
}