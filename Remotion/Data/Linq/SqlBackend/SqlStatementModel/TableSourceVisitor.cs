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
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="TableSourceVisitor"/> modifies <see cref="AbstractTableSource"/>s.
  /// </summary>
  public class TableSourceVisitor : ITableSourceVisitor
  {
    private readonly ISqlStatementResolver _resolver;

    public static void ReplaceTableSource (SqlTable sqlTable, ISqlStatementResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("resolver", resolver);

      var visitor = new TableSourceVisitor (resolver);

      if (sqlTable.TableSource is ConstantTableSource)
      {
        sqlTable.TableSource = visitor.VisitConstantTableSource ((ConstantTableSource) sqlTable.TableSource);
      }
      else if (sqlTable.TableSource is SqlTableSource)
        visitor.VisitSqlTableSource ((SqlTableSource) sqlTable.TableSource);
      else
        throw new NotSupportedException (string.Format ("SqlTable.TableSource of type '{0}' is not supported.", sqlTable.TableSource.GetType().Name));

    }

    protected TableSourceVisitor (ISqlStatementResolver resolver)
    {
      _resolver = resolver;
    }

    public AbstractTableSource VisitConstantTableSource (ConstantTableSource tableSource)
    {
      return  _resolver.ResolveConstantTableSource (tableSource);
    }

    public AbstractTableSource VisitSqlTableSource (SqlTableSource tableSource)
    {
      throw new NotImplementedException();
    }
  }
}