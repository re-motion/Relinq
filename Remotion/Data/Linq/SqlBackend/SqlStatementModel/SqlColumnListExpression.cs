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

// TODO: Move to SqlStatementModel.Resolved namespace
namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlColumnListExpression"/> holds a list of <see cref="SqlColumnExpression"/> instances.
  /// </summary>
  public class SqlColumnListExpression : ExtensionExpression
  {
    private readonly SqlColumnExpression[] _columns;

    public SqlColumnListExpression (Type type, SqlColumnExpression[] columns) // TODO: consider making parameter a params[] for convenience
        : base (type)
    {
      ArgumentUtility.CheckNotNull ("columns", columns);

      _columns = columns;
    }

    public ReadOnlyCollection<SqlColumnExpression> Columns
    {
      get { return Array.AsReadOnly(_columns); }
    }

    protected internal override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      // TODO: Change tests to check that if the visitor changes the columns, a new SqlColumnListExpression is created
      // TODO: Change code as follows:
      // var originalColumns = Columns;
      // var newColumns = visitor.VisitAndConvert (originalColumns);
      // if (newColumns != originalColumns)
      //   return new SqlColumnListExpression (Type, newColumns);
      // else
      //   return this;
      (visitor.VisitList (Columns, c => (SqlColumnExpression) visitor.VisitExpression (c))).CopyTo (_columns, 0);
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