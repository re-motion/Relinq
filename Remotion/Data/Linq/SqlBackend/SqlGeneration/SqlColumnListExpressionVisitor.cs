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
using System.Linq.Expressions;
using System.Text;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlColumnListExpressionVisitor"/> implements <see cref="ThrowingExpressionTreeVisitor"/> and <see cref="ISqlColumnListExpressionVisitor"/>.
  /// </summary>
  public class SqlColumnListExpressionVisitor : ThrowingExpressionTreeVisitor, ISqlColumnListExpressionVisitor
  {
    private readonly StringBuilder _sb;
    private bool _first;

    public static void TranslateSqlColumnListExpression (SqlColumnListExpression expression, StringBuilder sb)
    {
      var visitor = new SqlColumnListExpressionVisitor(sb);
      visitor.VisitExpression (expression);
    }

    protected SqlColumnListExpressionVisitor (StringBuilder sb)
    {
      ArgumentUtility.CheckNotNull ("sb", sb);
      _sb = sb;
    }

    public Expression VisitSqlColumListExpression (SqlColumnListExpression expression)
    {
      return BaseVisitUnknownExpression (expression);
    }

    public Expression VisitSqlColumnExpression (Expression expression)
    {
      var prefix = ((SqlTableSource) ((SqlColumnExpression) expression).SqlTable.TableSource).TableAlias;
      var columnName = ((SqlColumnExpression) expression).ColumnName;
      if (!_first)
        _sb.Append (string.Format ("[{0}].[{1}]", prefix, columnName));
      else
        _sb.Append (string.Format (",[{0}].[{1}]", prefix, columnName));
      _first = true;
      return expression; 
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      throw new NotImplementedException();
    }
  }
}