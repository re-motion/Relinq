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
using System.Collections.Generic;
using System.Linq;
using Remotion.Utilities;

namespace Remotion.Linq.Collections
{
  /// <summary>
  /// Defines extension methods that simplify working with a dictionary that has a collection-values item-type.
  /// </summary>
  internal static class MultiDictionaryExtensions
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