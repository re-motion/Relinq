// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Reflection;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// Provides a base class for SQL tables, both stand-alone tables and joined tables.
  /// </summary>
  public abstract class SqlTableBase
  {
    private readonly Dictionary<MemberInfo, SqlTable> _joinedTables = new Dictionary<MemberInfo, SqlTable>();
    private readonly Type _itemType;

    protected SqlTableBase (Type itemType)
    {
      ArgumentUtility.CheckNotNull ("itemType", itemType);

      _itemType = itemType;
    }

    public abstract SqlTableSource GetResolvedTableSource ();

    public Type ItemType 
    { 
      get { return _itemType; } 
    }

      public Dictionary<MemberInfo, SqlTable> JoinedTables // TODO: Remove
    {
      get { return _joinedTables; }
    }

    public SqlTable GetOrAddJoin (MemberInfo relationMember, JoinedTableSource tableSource)
    {
      if (ReflectionUtility.GetFieldOrPropertyType (relationMember) != tableSource.ItemType)
      {
        string message = string.Format (
            "Type mismatch between {0} and {1}.", ReflectionUtility.GetFieldOrPropertyType (relationMember).Name, tableSource.ItemType.Name);
        throw new InvalidOperationException (message);
      }

      if (!JoinedTables.ContainsKey (relationMember))
        JoinedTables.Add (relationMember, new SqlTable (tableSource));

      return JoinedTables[relationMember];
    }

    public SqlTable GetJoin (MemberInfo relationMember)
    {
      return JoinedTables[relationMember];
    }
  }
}