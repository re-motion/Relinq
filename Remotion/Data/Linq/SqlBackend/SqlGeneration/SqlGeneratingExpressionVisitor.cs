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

      var visitor = new SqlGeneratingExpressionVisitor (commandBuilder);
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
      _commandBuilder.Append (string.Format ("[{0}].[{1}]", prefix, columnName)); // TODO: Add _commandBuilder.AppendFormat calling StringBuilder.AppendFormat

      return expression;
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      // TODO: Perform check for null values here
      // TODO: Parameter name must be appended to _commandBuilder
      if (expression.Type == typeof (bool))
        _commandBuilder.AddParameter ((bool) expression.Value ? 1 : 0);
      else
        _commandBuilder.AddParameter (expression.Value);

      return expression;
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      _commandBuilder.Append ("(");
      VisitExpression (expression.Left);

      switch (expression.NodeType)
      {
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
          _commandBuilder.Append (" + ");
          break;
        case ExpressionType.And:
          throw new NotSupportedException();
        case ExpressionType.AndAlso:
          _commandBuilder.Append (" AND ");
          break;
        case ExpressionType.ArrayLength:
          throw new NotSupportedException();
        case ExpressionType.ArrayIndex:
          throw new NotSupportedException();
        case ExpressionType.Call:
          throw new NotSupportedException();
        case ExpressionType.Coalesce:
          throw new NotSupportedException();
        case ExpressionType.Conditional:
          throw new NotSupportedException();
        case ExpressionType.Constant:
          throw new NotSupportedException();
        case ExpressionType.Convert:
          throw new NotSupportedException();
        case ExpressionType.ConvertChecked:
          throw new NotSupportedException();
        case ExpressionType.Divide:
          _commandBuilder.Append (" / ");
          break;
        case ExpressionType.Equal:
          throw new NotSupportedException();
        case ExpressionType.ExclusiveOr:
          throw new NotSupportedException();
        case ExpressionType.GreaterThan:
          throw new NotSupportedException();
        case ExpressionType.GreaterThanOrEqual:
          throw new NotSupportedException();
        case ExpressionType.Invoke:
          throw new NotSupportedException();
        case ExpressionType.Lambda:
          throw new NotSupportedException();
        case ExpressionType.LeftShift:
          throw new NotSupportedException();
        case ExpressionType.LessThan:
          throw new NotSupportedException();
        case ExpressionType.LessThanOrEqual:
          throw new NotSupportedException();
        case ExpressionType.ListInit:
          throw new NotSupportedException();
        case ExpressionType.MemberAccess:
          throw new NotSupportedException();
        case ExpressionType.MemberInit:
          throw new NotSupportedException();
        case ExpressionType.Modulo:
          _commandBuilder.Append (" % ");
          break;
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
          _commandBuilder.Append (" * ");
          break;
        case ExpressionType.Negate:
          throw new NotSupportedException();
        case ExpressionType.UnaryPlus:
          throw new NotSupportedException();
        case ExpressionType.NegateChecked:
          throw new NotSupportedException();
        case ExpressionType.New:
          throw new NotSupportedException();
        case ExpressionType.NewArrayInit:
          throw new NotSupportedException();
        case ExpressionType.NewArrayBounds:
          throw new NotSupportedException();
        case ExpressionType.Not:
          throw new NotSupportedException();
        case ExpressionType.NotEqual:
          throw new NotSupportedException();
        case ExpressionType.Or:
          throw new NotSupportedException();
        case ExpressionType.OrElse:
          _commandBuilder.Append (" OR ");
          break;
        case ExpressionType.Parameter:
          throw new NotSupportedException();
        case ExpressionType.Power:
          throw new NotSupportedException();
        case ExpressionType.Quote:
          throw new NotSupportedException();
        case ExpressionType.RightShift:
          throw new NotSupportedException();
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
          _commandBuilder.Append (" - ");
          break;
        case ExpressionType.TypeAs:
          throw new NotSupportedException();
        case ExpressionType.TypeIs:
          throw new NotSupportedException();
        default:
          throw new ArgumentOutOfRangeException();
      }

      VisitExpression (expression.Right);
      _commandBuilder.Append (")");
      return expression;
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      throw new NotSupportedException (
          string.Format (
              "The expression '{0}' cannot be translated to SQL text by this SQL generator. Expression type '{1}' is not supported.",
              unhandledItem,
              unhandledItem.GetType().Name));
    }
  }
}