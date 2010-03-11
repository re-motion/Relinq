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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration.BooleanSemantics
{
  /// <summary>
  /// Ensures that a given expression matches SQL server value semantics.
  /// </summary>
  public class BooleanSemanticsExpressionConverter : ExpressionTreeVisitor, IResolvedSqlExpressionVisitor, ISqlSpecificExpressionVisitor
  {
    public static Expression ConvertBooleanExpressions (Expression expression, BooleanSemanticsKind initialSemantics)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var visitor = new BooleanSemanticsExpressionConverter (initialSemantics);
      var result = visitor.VisitExpression (expression);
      return result;
    }

    private readonly BooleanSemanticsHolder _semantics;

    protected BooleanSemanticsExpressionConverter (BooleanSemanticsKind initialSemantics)
    {
      _semantics = new BooleanSemanticsHolder (initialSemantics);
    }

    public override Expression VisitExpression (Expression expression)
    {
      if (expression == null)
        return null;

      var result = base.VisitExpression (expression);

      if (_semantics.CurrentValue == BooleanSemanticsKind.ValueRequired)
        return EnsureValueSemantics (result);
      else if (_semantics.CurrentValue == BooleanSemanticsKind.PredicateRequired)
        return EnsurePredicateSemantics (result);
      else
        throw new NotImplementedException ("Invalid enum value: " + _semantics.CurrentValue);
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (bool))
        return expression;

      Expression expressionAsValue = expression.Value.Equals (true) ? Expression.Constant (1) : Expression.Constant (0);

      return expressionAsValue;
    }

    public Expression VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (bool))
        return expression;

      var expressionAsValue = new SqlColumnExpression (typeof (int), expression.OwningTableAlias, expression.ColumnName);
      return expressionAsValue;
    }

    public Expression VisitSqlCaseExpression (SqlCaseExpression expression)
    {
      var testPredicate = ConvertBooleanExpressions (expression.TestPredicate, BooleanSemanticsKind.PredicateRequired);
      var thenValue = ConvertBooleanExpressions (expression.ThenValue, BooleanSemanticsKind.ValueRequired);
      var elseValue = ConvertBooleanExpressions (expression.ElseValue, BooleanSemanticsKind.ValueRequired);

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

      var left = ConvertBooleanExpressions (expression.Left, childSemantics);
      var right = ConvertBooleanExpressions (expression.Right, childSemantics);

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

      var operand = ConvertBooleanExpressions (expression.Operand, childSemantics);

      if (operand != expression.Operand)
        expression = Expression.MakeUnary (expression.NodeType, operand, expression.Type, expression.Method);

      return expression;
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlColumnListExpression (SqlColumnListExpression expression)
    {
      return base.VisitUnknownExpression (expression);
    }

    Expression ISqlSpecificExpressionVisitor.VisitSqlLiteralExpression (SqlLiteralExpression expression)
    {
      return VisitUnknownExpression (expression);
    }

    private BooleanSemanticsKind GetChildSemanticsForBoolExpression (ExpressionType expressionType)
    {
      switch (expressionType)
      {
        case ExpressionType.NotEqual:
        case ExpressionType.Equal:
          return BooleanSemanticsKind.ValueRequired;

        case ExpressionType.AndAlso:
        case ExpressionType.OrElse:
        case ExpressionType.And:
        case ExpressionType.Or:
        case ExpressionType.ExclusiveOr:
          return BooleanSemanticsKind.PredicateRequired;

        case ExpressionType.Not:
          return BooleanSemanticsKind.PredicateRequired;

        case ExpressionType.Convert:
          var message = string.Format ("'{0}' expressions are not supported with boolean type.", expressionType);
          throw new NotSupportedException (message);

        default:
          return BooleanSemanticsKind.ValueRequired;
      }
    }

    private static Expression EnsureValueSemantics (Expression expression)
    {
      if (expression.Type == typeof (bool))
        return new SqlCaseExpression (expression, new SqlLiteralExpression (1), new SqlLiteralExpression (0));
      else
        return expression;
    }

    private static Expression EnsurePredicateSemantics (Expression result)
    {
      if (result.Type == typeof (bool))
        return result;
      else if (result.Type == typeof (int))
        return Expression.Equal (result, new SqlLiteralExpression (1));
      else
        throw new NotSupportedException (string.Format ("Cannot convert an expression of type '{0}' to a boolean expression.", result.Type));
    }
  }
}