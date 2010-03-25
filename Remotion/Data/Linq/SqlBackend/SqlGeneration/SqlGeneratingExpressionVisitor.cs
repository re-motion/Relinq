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
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlGeneratingExpressionVisitor"/> implements <see cref="ThrowingExpressionTreeVisitor"/> and <see cref="IResolvedSqlExpressionVisitor"/>.
  /// </summary>
  public class SqlGeneratingExpressionVisitor
      : ThrowingExpressionTreeVisitor, IResolvedSqlExpressionVisitor, ISqlSpecificExpressionVisitor, ISqlSubStatementExpressionVisitor
  {
    public static void GenerateSql (
        Expression expression,
        SqlCommandBuilder commandBuilder,
        MethodCallSqlGeneratorRegistry methodCallRegistry,
        SqlExpressionContext context,
        ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("methodCallRegistry", methodCallRegistry);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var expressionWithBooleanSemantics = SqlContextExpressionVisitor.ApplySqlExpressionContext (expression, context);

      var visitor = new SqlGeneratingExpressionVisitor (commandBuilder, methodCallRegistry, stage);
      visitor.VisitExpression (expressionWithBooleanSemantics);
    }

    private readonly SqlCommandBuilder _commandBuilder;
    private readonly MethodCallSqlGeneratorRegistry _methodCallRegistry;
    private readonly BinaryExpressionTextGenerator _binaryExpressionTextGenerator;
    private readonly ISqlGenerationStage _stage;

    protected SqlGeneratingExpressionVisitor (
        SqlCommandBuilder commandBuilder, MethodCallSqlGeneratorRegistry methodCallRegistry, ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("methodCallRegistry", methodCallRegistry);
      ArgumentUtility.CheckNotNull ("stage", stage);

      _commandBuilder = commandBuilder;
      _methodCallRegistry = methodCallRegistry;
      _binaryExpressionTextGenerator = new BinaryExpressionTextGenerator (commandBuilder, this);
      _stage = stage;
    }

    public Expression VisitSqlEntityExpression (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var first = true;
      foreach (var column in expression.ProjectionColumns)
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
      _commandBuilder.AppendFormat ("[{0}].[{1}]", prefix, columnName);

      return expression;
    }

    public Expression VisitSqlEntityConstantExpression (SqlEntityConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var parameter = _commandBuilder.AddParameter (expression.PrimaryKeyValue);
      _commandBuilder.Append (parameter.Name);

      return expression;
    }

    public Expression VisitJoinConditionExpression (JoinConditionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var whereExpression = Expression.Equal (
          ((ResolvedJoinInfo) expression.JoinTable.JoinInfo).PrimaryColumn, 
          ((ResolvedJoinInfo) expression.JoinTable.JoinInfo).ForeignColumn);
      VisitExpression (whereExpression);
      return expression;
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      Debug.Assert (expression.Type != typeof (bool), "Boolean constants should have been removed by SqlContextExpressionVisitor.");

      if (expression.Value == null)
        _commandBuilder.Append ("NULL");
      else
      {
        var parameter = _commandBuilder.AddParameter (expression.Value);
        _commandBuilder.Append (parameter.Name);
      }

      return expression;
    }

    public Expression VisitSqlLiteralExpression (SqlLiteralExpression expression)
    {
      _commandBuilder.Append (expression.Value.ToString());
      return expression;
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      _commandBuilder.Append ("(");
      _binaryExpressionTextGenerator.GenerateSqlForBinaryExpression (expression);
      _commandBuilder.Append (")");
      return expression;
    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      switch (expression.NodeType)
      {
        case ExpressionType.Not:
          if (expression.Operand.Type == typeof (bool))
            _commandBuilder.Append ("NOT ");
          else
            _commandBuilder.Append ("~");
          break;
        case ExpressionType.Negate:
          _commandBuilder.Append ("-");
          break;
        case ExpressionType.UnaryPlus:
          _commandBuilder.Append ("+");
          break;
        default:
          throw new NotSupportedException();
      }

      VisitExpression (expression.Operand);

      return expression;
    }

    protected override Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      var generator = _methodCallRegistry.GetGenerator (expression.Method);
      generator.GenerateSql (expression, _commandBuilder, this);

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

    public Expression VisitSqlCaseExpression (SqlCaseExpression expression)
    {
      _commandBuilder.Append ("CASE WHEN ");
      VisitExpression (expression.TestPredicate);
      _commandBuilder.Append (" THEN ");
      VisitExpression (expression.ThenValue);
      _commandBuilder.Append (" ELSE ");
      VisitExpression (expression.ElseValue);
      _commandBuilder.Append (" END");
      return expression;
    }

    public Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      _commandBuilder.Append ("(");
      _stage.GenerateTextForSqlStatement (_commandBuilder, expression.SqlStatement);
      _commandBuilder.Append (")");
      return expression;
    }
  }
}