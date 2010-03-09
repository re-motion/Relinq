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
    private readonly SqlTableSource _foreignTableSource;
    private readonly SqlColumnExpression _primaryColumn;
    private readonly SqlColumnExpression _foreignColumn;

    public SqlJoinedTableSource (SqlTableSource foreignTableSource, SqlColumnExpression primaryColumn, SqlColumnExpression foreignColumn)
    {
      ArgumentUtility.CheckNotNull ("foreignTableSource", foreignTableSource);
      ArgumentUtility.CheckNotNull ("primaryColumn", primaryColumn);
      ArgumentUtility.CheckNotNull ("foreignColumn", foreignColumn);
      
      _foreignTableSource = foreignTableSource;
      _primaryColumn = primaryColumn;
      _foreignColumn = foreignColumn;
    }

    public override Type ItemType
    {
      get { return _foreignTableSource.ItemType; }
    }

    public SqlTableSource ForeignTableSource
    {
      get { return _foreignTableSource; }
    }

    public SqlColumnExpression PrimaryColumn
    {
      get { return _primaryColumn; }
    }

    public SqlColumnExpression ForeignColumn
    {
      get { return _foreignColumn; }
    }

    public override AbstractTableSource Accept (ITableSourceVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitSqlJoinedTableSource (this);
    }

    public override SqlTableSource GetResolvedTableSource ()
    {
      return ForeignTableSource;
    }
  }
}