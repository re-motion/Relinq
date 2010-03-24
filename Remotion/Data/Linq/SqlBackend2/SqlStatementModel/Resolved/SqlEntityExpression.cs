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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;
using System.Linq;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="SqlEntityExpression"/> holds a list of <see cref="SqlColumnExpression"/> instances.
  /// </summary>
  public class SqlEntityExpression : ExtensionExpression
  {
    private readonly SqlColumnExpression _primaryKeyColumn;
    private readonly ReadOnlyCollection<SqlColumnExpression> _projectionColumns;
    
    public SqlEntityExpression (Type type, SqlColumnExpression primaryKeyColumn, params SqlColumnExpression[] projectionColumns)
        : base (type)
    {
      ArgumentUtility.CheckNotNull ("projectionColumns", projectionColumns);
      ArgumentUtility.CheckNotNull ("primaryKeyColumn", primaryKeyColumn);

      _projectionColumns = Array.AsReadOnly (projectionColumns);
      _primaryKeyColumn = primaryKeyColumn;
    }

    public SqlColumnExpression PrimaryKeyColumn
    {
      get { return _primaryKeyColumn; }
    }

    public ReadOnlyCollection<SqlColumnExpression> ProjectionColumns
    {
      get { return _projectionColumns; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      // TODO: PrimaryKeyColumn must be visited.
      var newColumns = visitor.VisitAndConvert (ProjectionColumns, "VisitChildren");
      if (newColumns != ProjectionColumns)
        return new SqlEntityExpression (Type, PrimaryKeyColumn, newColumns.ToArray());
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as IResolvedSqlExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlEntityExpression (this);
      else
        return base.Accept (visitor);
    }
  }
}