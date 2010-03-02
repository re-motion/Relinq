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

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlJoinedTableSource"/> represents a join between two database tables.
  /// </summary>
  public class SqlJoinedTableSource : AbstractTableSource
  {
    private readonly Type _type;
    private readonly SqlTableSource _primaryTableSource;
    private readonly SqlTableSource _foreignTableSource;
    private readonly string _primaryKey;
    private readonly string _foreignKey;

    public SqlJoinedTableSource (SqlTableSource primaryTableSource, SqlTableSource foreignTableSource, string primaryKey, string foreignKey, Type type)
    {
      ArgumentUtility.CheckNotNull ("primaryTableSource", primaryTableSource);
      ArgumentUtility.CheckNotNull ("foreignTableSource", foreignTableSource);
      ArgumentUtility.CheckNotNullOrEmpty ("primaryKey", primaryKey);
      ArgumentUtility.CheckNotNullOrEmpty ("foreignKey", foreignKey);
      ArgumentUtility.CheckNotNull ("type", type);

      _primaryTableSource = primaryTableSource;
      _foreignTableSource = foreignTableSource;
      _primaryKey = primaryKey;
      _foreignKey = foreignKey;
      _type = type;
    }
    
    public override Type Type
    {
      get { return _type; }
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