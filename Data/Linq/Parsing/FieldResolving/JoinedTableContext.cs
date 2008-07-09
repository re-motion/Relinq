/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Specialized;
using System.Reflection;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  public class JoinedTableContext
  {
    private readonly OrderedDictionary _tables = new OrderedDictionary ();
    private int _generatedAliasCount = 0;

    public int Count
    {
      get { return _tables.Count; }
    }

    public Table GetJoinedTable (IDatabaseInfo databaseInfo, FieldSourcePath fieldSourcePath, MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("fieldSourcePath", fieldSourcePath);
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      Tuple<FieldSourcePath, MemberInfo> key = Tuple.NewTuple (fieldSourcePath, relationMember);

      if (!_tables.Contains (key))
        _tables.Add (key, DatabaseInfoUtility.GetRelatedTable (databaseInfo, relationMember));
      return (Table) _tables[key];
    }

    public void CreateAliases ()
    {
      for (int i = 0; i < Count; ++i)
      {
        Table table = (Table) _tables[i];
        if (table.Alias == null)
        {
          table.SetAlias ("j" + _generatedAliasCount);
          ++_generatedAliasCount;
        }
      }
    }
  }
}
