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
using System.Collections;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Ensures that a given expression matches SQL server value semantics.
  /// </summary>
  /// <remarks>
  /// <see cref="SqlContextExpressionVisitor"/> traverses an expression tree and ensures that the tree fits SQL server requirements for
  /// boolean expressions. In scenarios where a value is required as per SQL server standards, bool expressions are converted to integers using
  /// CASE WHEN expressions. In such situations, <see langword="true" /> and <see langword="false" /> constants are converted to 1 and 0 values,
  /// and boolean columns are interpreted as integer values. In scenarios where a predicate is required, boolean expressions are constructed by 
  /// comparing those integer values to 1 and 0 literals.
  /// </remarks>
  public class SqlContextExpressionVisitor : ExpressionTreeVisitor, ISqlSpecificExpressionVisitor
  {
    private static readonly SqlContextExpressionVisitor s_visitorInstance = new SqlContextExpressionVisitor ();

    public static Expression ApplySqlExpressionContext (Expression expression, SqlExpressionContext initialSemantics)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var result = s_visitorInstance.VisitExpression (expression);

      if (initialSemantics == SqlExpressionContext.ValueRequired)
      {
        return EnsureValueSemantics (result);
      }
      else if (initialSemantics == SqlExpressionContext.PredicateRequired)
        return EnsurePredicateSemantics (result);
      else
        throw new NotImplementedException ("Invalid enum value: " + initialSemantics);
    }

    // TODO: Add EnsureSingleValueSemantics (...)
    // TODO: entityExpression = expression as SqlEntityExpression; if (entityExpression != null) { return entityExpression.PrimaryKeyColumn; }
    // TODO: Else call EnsureValueSemtantics

    private static Expression EnsureValueSemantics (Expression expression)
    {
      if (expression.Type != typeof (string) && typeof (IEnumerable).IsAssignableFrom (expression.Type))
        throw new NotSupportedException ("Subquery selects a collection where a single value is expected.");

      if (expression.Type == typeof (bool))
      {
        if (expression.NodeType == ExpressionType.Constant)
          return ConvertBoolConstantToValue ((ConstantExpression) expression);
        else if (expression is SqlColumnExpression)
          return ConvertSqlColumnToValue ((SqlColumnExpression) expression);
        else
          return new SqlCaseExpression (expression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
      }
      else
      {
        return expression;
      }
    }

    private static Expression EnsurePredicateSemantics (Expression expression)
    {
      if (expression.Type == typeof (bool))
      {
        if (expression.NodeType == ExpressionType.Constant)
          return ConvertBoolConstantToPredicate ((ConstantExpression) expression);
        else if (expression is SqlColumnExpression)
          return ConvertSqlColumnToPredicate ((SqlColumnExpression) expression);
        else
          return expression;
      }
      else if (expression.Type == typeof (int))
        return Expression.Equal (expression, new SqlLiteralExpression (1));
      else
        throw new NotSupportedException (string.Format ("Cannot convert an expression of type '{0}' to a boolean expression.", expression.Type));
    }

    private static Expression ConvertBoolConstantToValue (ConstantExpression expression)
    {
      return expression.Value.Equals (true) ? Expression.Constant (1) : Expression.Constant (0);
    }

    private static Expression ConvertBoolConstantToPredicate (ConstantExpression expression)
    {
      var expressionAsValue = ConvertBoolConstantToValue (expression); // 1/0
      return EnsurePredicateSemantics (expressionAsValue); // 1/0 == 1
    }

    private static Expression ConvertSqlColumnToValue (SqlColumnExpression expression)
    {
      return new SqlColumnExpression (typeof (int), expression.OwningTableAlias, expression.ColumnName);
    }

    private static Expression ConvertSqlColumnToPredicate (SqlColumnExpression expression)
    {
      var expressionAsValue = ConvertSqlColumnToValue (expression); // int column
      return EnsurePredicateSemantics (expressionAsValue); // int column == 1
    }

    protected SqlContextExpressionVisitor ()
    {
    }

    // The Visit methods model the rules of where what kind of expression context is required

    public Expression VisitSqlCaseExpression (SqlCaseExpression expression)
    {
      var testPredicate = ApplySqlExpressionContext (expression.TestPredicate, SqlExpressionContext.PredicateRequired);
      var thenValue = ApplySqlExpressionContext (expression.ThenValue, SqlExpressionContext.ValueRequired);
      var elseValue = ApplySqlExpressionContext (expression.ElseValue, SqlExpressionContext.ValueRequired);

      if (testPredicate != expression.TestPredicate || thenValue != expression.ThenValue || elseValue != expression.ElseValue)
        return new SqlCaseExpression (testPredicate, thenValue, elseValue);
      else
        return expression;
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (bool))
        return base.VisitBinaryExpression (expression);

      var childSemantics = GetChildSemanticsForBoolExpression (expression.NodeType);

      var left = ApplySqlExpressionContext (expression.Left, childSemantics);
      var right = ApplySqlExpressionContext (expression.Right, childSemantics);

      if (left != expression.Left || right != expression.Right)
        expression = Expression.MakeBinary (expression.NodeType, left, right);

      return expression;
    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (bool))
        return base.VisitUnaryExpression (expression);

      var childSemantics = GetChildSemanticsForBoolExpression (expression.NodeType);

      var operand = ApplySqlExpressionContext (expression.Operand, childSemantics);

      if (operand != expression.Operand)
        expression = Expression.MakeUnary (expression.NodeType, operand, expression.Type, expression.Method);

      return expression;
    }

    Expression ISqlSpecificExpressionVisitor.VisitSqlLiteralExpression (SqlLiteralExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    private SqlExpressionContext GetChildSemanticsForBoolExpression (ExpressionType expressionType)
    {
      switch (expressionType)
      {
        case ExpressionType.NotEqual:
        case ExpressionType.Equal:
          return SqlExpressionContext.ValueRequired;

        case ExpressionType.AndAlso:
        case ExpressionType.OrElse:
        case ExpressionType.And:
        case ExpressionType.Or:
        case ExpressionType.ExclusiveOr:
          return SqlExpressionContext.PredicateRequired;

        case ExpressionType.Not:
          return SqlExpressionContext.PredicateRequired;

        case ExpressionType.Convert:
          var message = string.Format ("'{0}' expressions are not supported with boolean type.", expressionType);
          throw new NotSupportedException (message);

        default:
          return SqlExpressionContext.ValueRequired;
      }
    }
  }
}