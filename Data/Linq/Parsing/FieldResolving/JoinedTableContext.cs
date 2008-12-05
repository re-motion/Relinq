// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
//
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
//
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
//
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
