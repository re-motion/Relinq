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
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="SqlJoinedTableSource"/> represents a join between two database tables.
  /// </summary>
  public class SqlJoinedTableSource : AbstractTableSource
  {
    private readonly SqlTableSource _primaryTableSource;
    private readonly SqlTableSource _foreignTableSource;
    private readonly string _primaryKey;
    private readonly string _foreignKey;

    // TODO: UseSqlColumnExpression for primary key and foreign key, remove primaryTableSource, make foreignTableSource an AbstractTableSource.
    public SqlJoinedTableSource (SqlTableSource primaryTableSource, SqlTableSource foreignTableSource, string primaryKey, string foreignKey)
    {
      ArgumentUtility.CheckNotNull ("primaryTableSource", primaryTableSource);
      ArgumentUtility.CheckNotNull ("foreignTableSource", foreignTableSource);
      ArgumentUtility.CheckNotNullOrEmpty ("primaryKey", primaryKey);
      ArgumentUtility.CheckNotNullOrEmpty ("foreignKey", foreignKey);
      
      _primaryTableSource = primaryTableSource;
      _foreignTableSource = foreignTableSource;
      _primaryKey = primaryKey;
      _foreignKey = foreignKey;

    }

    public override Type ItemType
    {
      get { return _foreignTableSource.ItemType; }
    }

    public SqlTableSource PrimaryTableSource
    {
      get { return _primaryTableSource; }
    }

    public SqlTableSource ForeignTableSource
    {
      get { return _foreignTableSource; }
    }

    public string PrimaryKey
    {
      get { return _primaryKey; }
    }

    public string ForeignKey
    {
      get { return _foreignKey; }
    }

    public override AbstractTableSource Accept (ITableSourceVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitSqlJoinedTableSource (this);
    }
  }
}