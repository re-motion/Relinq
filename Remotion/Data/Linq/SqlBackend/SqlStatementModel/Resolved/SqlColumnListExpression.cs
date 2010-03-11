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
  /// <see cref="SqlColumnListExpression"/> holds a list of <see cref="SqlColumnExpression"/> instances.
  /// </summary>
  public class SqlColumnListExpression : ExtensionExpression
  {
    private readonly ReadOnlyCollection<SqlColumnExpression> _columns;

    public SqlColumnListExpression (Type type, params SqlColumnExpression[] columns)
        : base (type)
    {
      ArgumentUtility.CheckNotNull ("columns", columns);

      _columns = Array.AsReadOnly (columns);
    }

    public ReadOnlyCollection<SqlColumnExpression> Columns
    {
      get { return _columns; }
    }

    protected internal override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newColumns = visitor.VisitAndConvert (Columns, "VisitChildren");
      if (newColumns != Columns)
        return new SqlColumnListExpression (Type, newColumns.ToArray());
      else
        return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as IResolvedSqlExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlColumnListExpression (this);
      else
        return base.Accept (visitor);
    }
  }
}