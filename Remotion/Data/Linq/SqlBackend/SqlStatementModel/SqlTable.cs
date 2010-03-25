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
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlTable"/> represents a data source in a <see cref="SqlStatement"/>.
  /// </summary>
  public class SqlTable : SqlTableBase
  {
    private AbstractTableInfo _tableInfo;

    public SqlTable (AbstractTableInfo tableInfo) 
        : base (tableInfo.ItemType)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      _tableInfo = tableInfo;
    }

    public AbstractTableInfo TableInfo
    {
      get { return _tableInfo; }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        if (_tableInfo != null)
        {
          if (_tableInfo.ItemType != value.ItemType)
            throw new ArgumentTypeException ("value", _tableInfo.ItemType, value.ItemType);
        }
        _tableInfo = value;
      }
    }

    public override void Accept (ISqlTableBaseVisitor visitor)
    {
      visitor.VisitSqlTable (this);
    }

    public override IResolvedTableInfo GetResolvedTableInfo ()
    {
      return TableInfo.GetResolvedTableInfo();
    }
  }
}