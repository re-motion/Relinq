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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlGeneratingExpressionVisitor"/> implements <see cref="ThrowingExpressionTreeVisitor"/> and <see cref="ISqlColumnListExpressionVisitor"/>.
  /// </summary>
  public class SqlGeneratingExpressionVisitor : ThrowingExpressionTreeVisitor, ISqlColumnListExpressionVisitor
  {
    private readonly SqlCommandBuilder _commandBuilder;

    public static void GenerateSql (Expression expression, SqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      var visitor = new SqlGeneratingExpressionVisitor(commandBuilder);
      visitor.VisitExpression (expression);
    }

    protected SqlGeneratingExpressionVisitor (SqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      _commandBuilder = commandBuilder;
    }

    public Expression VisitSqlColumListExpression (SqlColumnListExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var first = true;
      foreach (var column in expression.Columns)
      {
        if (!first)
          _commandBuilder.Append (",");
        column.Accept (this);
        first = false;
      }

      return expression;
    }

    public Expression VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var prefix = expression.OwningTableAlias;
      var columnName = expression.ColumnName;
      _commandBuilder.Append (string.Format ("[{0}].[{1}]", prefix, columnName));

      return expression; 
    }

    //protected override Expression VisitConstantExpression (ConstantExpression expression)
    //{
    //  return base.VisitConstantExpression (expression);
    //}

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      throw new NotSupportedException (
          string.Format ("The expression '{0}' cannot be translated to SQL text by this SQL generator. Expression type '{1}' is not supported.",unhandledItem,unhandledItem.GetType().Name));
    }
  }
}