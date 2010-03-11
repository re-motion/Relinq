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
      return visitor.VisitExpression (expression);
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
        return result;
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
      Expression testPredicate;
      Expression thenValue;
      Expression elseValue;

      using (_semantics.SwitchTo (BooleanSemanticsKind.PredicateRequired))
      {
        testPredicate = VisitExpression (expression.TestPredicate);
      }

      using (_semantics.SwitchTo (BooleanSemanticsKind.ValueRequired))
      {
        thenValue = VisitExpression (expression.ThenValue);
        elseValue = VisitExpression (expression.ElseValue);
      }

      if (testPredicate != expression.TestPredicate || thenValue != expression.ThenValue || elseValue != expression.ElseValue)
        return new SqlCaseExpression (testPredicate, thenValue, elseValue);
      else
        return expression;
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (bool))
      {
        Debug.Assert (_semantics.CurrentValue == BooleanSemanticsKind.ValueRequired); // ensured by check in VisitExpression
        return base.VisitBinaryExpression (expression);
      }

      var left = expression.Left;
      var right = expression.Right;

      switch (expression.NodeType)
      {
        case ExpressionType.NotEqual:
        case ExpressionType.Equal:
          using (_semantics.SwitchTo (BooleanSemanticsKind.ValueRequired))
          {
            left = VisitExpression (left);
            right = VisitExpression (right);
          }
          break;
        case ExpressionType.AndAlso:
        case ExpressionType.OrElse:
        case ExpressionType.And:
        case ExpressionType.Or:
        case ExpressionType.ExclusiveOr:
          using (_semantics.SwitchTo (BooleanSemanticsKind.PredicateRequired))
          {
            left = VisitExpression (left);
            right = VisitExpression (right);
          }
          break;
        default:
          Debug.Assert (false, string.Format ("Expression type '{0}' was not expected to have boolean type.", expression.NodeType));
          break;
      }

      if (left != expression.Left || right != expression.Right)
        expression = Expression.MakeBinary (expression.NodeType, left, right);

      return expression;
    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (bool))
      {
        Debug.Assert (_semantics.CurrentValue == BooleanSemanticsKind.ValueRequired); // ensured by check in VisitExpression
        return base.VisitUnaryExpression (expression);
      }

      var operand = expression.Operand;

      switch (expression.NodeType)
      {
        case ExpressionType.Not:
          using (_semantics.SwitchTo (BooleanSemanticsKind.PredicateRequired))
          {
            operand = VisitExpression (operand);
          }
          break;

        default:
          var message = string.Format ("'{0}' expressions are not supported with boolean type.", expression.NodeType);
          throw new NotSupportedException (message);
      }

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