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
using System.Text;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlTableSourceVisitor"/> generates sql-text for <see cref="SqlTableSource"/>.
  /// </summary>
  public class SqlTableSourceVisitor : ITableSourceVisitor
  {
    private readonly StringBuilder _sb;

    public static void GenerateSql (SqlTable sqlTable, StringBuilder sb)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("sb", sb);

      var visitor = new SqlTableSourceVisitor (sb);
      visitor.VisitTableSource (sqlTable.TableSource);
    }

    protected SqlTableSourceVisitor (StringBuilder sb)
    {
      ArgumentUtility.CheckNotNull ("sb", sb);
      _sb = sb;
    }

    public AbstractTableSource VisitTableSource (AbstractTableSource tableSource)
    {
      ArgumentUtility.CheckNotNull ("tableSource", tableSource);
      return tableSource.Accept (this);
    }

    public AbstractTableSource VisitConstantTableSource (ConstantTableSource tableSource)
    {
      throw new InvalidOperationException ("ConstantTableSource is not valid at this point.");
    }

    public AbstractTableSource VisitSqlTableSource (SqlTableSource tableSource)
    {
      _sb.Append ("[");
      _sb.Append (tableSource.TableName);
      _sb.Append ("]");
      _sb.Append (" AS ");
      _sb.Append ("[");
      _sb.Append (tableSource.TableAlias);
      _sb.Append ("]");

      return tableSource;
    }

    public AbstractTableSource VisitJoinedTableSource (JoinedTableSource tableSource)
    {
      throw new NotImplementedException();
    }
  }
}