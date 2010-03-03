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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlTable"/> represents a data source in a <see cref="SqlStatement"/>.
  /// </summary>
  public class SqlTable
  {
    private AbstractTableSource _tableSource; // TODO: Initialize from ctor parameter
    private readonly Dictionary<MemberInfo, SqlTable> _joinedTables;

    public SqlTable ()
    {
      _joinedTables = new Dictionary<MemberInfo, SqlTable>();
    }

    public AbstractTableSource TableSource
    {
      get { return _tableSource; }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        if (_tableSource != null)
        {
          if (_tableSource.ItemType != value.ItemType)
            throw new ArgumentTypeException ("value", _tableSource.ItemType, value.ItemType);
        }
        _tableSource = value;
      }
    }

    public SqlTable GetOrAddJoin (MemberInfo relationMember, JoinedTableSource tableSource)
    {
      if (ReflectionUtility.GetFieldOrPropertyType (relationMember) != tableSource.ItemType)
      {
        string message = string.Format (
            "Type mismatch between {0} and {1}.", ReflectionUtility.GetFieldOrPropertyType (relationMember).Name, tableSource.ItemType.Name);
        throw new InvalidOperationException (message);
      }

      if (!_joinedTables.ContainsKey (relationMember))
        _joinedTables.Add (relationMember, new SqlTable { TableSource = tableSource });

      return _joinedTables[relationMember];
    }
  }
}