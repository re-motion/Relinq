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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlColumnListExpression"/> holds a list of <see cref="SqlColumnExpression"/> instances.
  /// </summary>
  public class SqlColumnListExpression : ExtensionExpression
  {
    private readonly SqlColumnExpression[] _columns;

    public SqlColumnListExpression (Type type, SqlColumnExpression[] columns)
        : base (type)
    {
      ArgumentUtility.CheckNotNull ("columns", columns);

      _columns = columns;
    }

    public ReadOnlyCollection<SqlColumnExpression> Columns
    {
      get { return Array.AsReadOnly(_columns); }
    }

    // TODO: Implement and test - should call visitor.VisitExpression for all _columns.
    protected internal override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      // TODO: Refactor as soon as ExpressionTreeVisitor.VisitList is public.
      var newColumns = new List<SqlColumnExpression>();
      bool isAnyColumnChanged = false;

      foreach (var columnExpression in _columns)
      {
        var newColumnExpression = visitor.VisitExpression (columnExpression);  
        if (newColumnExpression != columnExpression)
          isAnyColumnChanged = true;
        newColumns.Add ((SqlColumnExpression) newColumnExpression);
      }

      if (isAnyColumnChanged)
        return new SqlColumnListExpression (Type, newColumns.ToArray());
      else
        return this;
      
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlColumnListExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlColumListExpression (this);
      else
        return base.Accept (visitor);
    }
  }
}