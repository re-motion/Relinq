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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Remotion.Linq.Utilities
{
  /// <summary>
  /// <see cref="RegistryBase{TRegistry,TKey,TItem}"/> provides code which is common in all registry classes.
  /// </summary>
  public abstract class RegistryBase<TRegistry, TKey, TItem>
      where TRegistry: RegistryBase<TRegistry, TKey, TItem>, new()
  {
    public static TRegistry CreateDefault ()
    {
      var defaultItemTypes = from t in typeof (TRegistry).Assembly.GetTypes() 
                             where typeof (TItem).IsAssignableFrom (t) && !t.IsAbstract
                             select t;

      var registry = new TRegistry();
      registry.RegisterForTypes (defaultItemTypes);
      return registry;
    }

    private readonly Dictionary<TKey, TItem> _items = new Dictionary<TKey, TItem>();

    public virtual void Register (TKey key, TItem item)
    {
      ArgumentUtility.CheckNotNull ("key", key);
      ArgumentUtility.CheckNotNull ("item", item);

      _items[key] = item;
    }

    public void Register (IEnumerable<TKey> keys, TItem item)
    {
      ArgumentUtility.CheckNotNull ("keys", keys);
      ArgumentUtility.CheckNotNull ("item", item);

      foreach (var key in keys)
        Register (key, item);
    }

    public abstract TItem GetItem (TKey key);

    public virtual bool IsRegistered (TKey key)
    {
      return _items.ContainsKey (key);
    }

    protected virtual TItem GetItemExact (TKey key)
    {
      TItem item;
      _items.TryGetValue (key, out item);
      return item;
    }

    protected abstract void RegisterForTypes (IEnumerable<Type> itemTypes);

  }
}