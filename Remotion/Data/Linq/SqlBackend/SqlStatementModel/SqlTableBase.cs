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
    private readonly Dictionary<MemberInfo, SqlJoinedTable> _joinedTables = new Dictionary<MemberInfo, SqlJoinedTable> ();
    private readonly Type _itemType;

    protected SqlTableBase (Type itemType)
    {
      ArgumentUtility.CheckNotNull ("itemType", itemType);

      _itemType = itemType;
    }

    public abstract ResolvedTableInfo GetResolvedTableInfo ();

    public Type ItemType 
    { 
      get { return _itemType; } 
    }

      public Dictionary<MemberInfo, SqlJoinedTable> JoinedTables // TODO: Remove
    {
      get { return _joinedTables; }
    }

      public SqlJoinedTable GetOrAddJoin (MemberInfo relationMember, UnresolvedJoinInfo joinInfo)
    {
      if (ReflectionUtility.GetFieldOrPropertyType (relationMember) != joinInfo.ItemType)
      {
        string message = string.Format (
            "Type mismatch between {0} and {1}.", ReflectionUtility.GetFieldOrPropertyType (relationMember).Name, joinInfo.ItemType.Name);
        throw new InvalidOperationException (message);
      }

      if (!JoinedTables.ContainsKey (relationMember))
        JoinedTables.Add (relationMember, new SqlJoinedTable (joinInfo));

      return JoinedTables[relationMember];
    }

      public SqlJoinedTable GetJoin (MemberInfo relationMember)
    {
      return JoinedTables[relationMember];
    }
  }
}