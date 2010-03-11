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
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration.BooleanSemantics
{
  /// <summary>
  /// Ensures that a given expression matches SQL server value semantics.
  /// </summary>
  public class BooleanSemanticsExpressionConverter : ExpressionTreeVisitor, IResolvedSqlExpressionVisitor
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
      if (expression.Type != typeof (bool) && _semantics.CurrentValue == BooleanSemanticsKind.PredicateRequired)
      {
        throw new InvalidOperationException ("It is not allowed to specify a non-boolean expression when a predicate is required.");
      }

      var result = base.VisitExpression (expression);

      if (result.Type == typeof (bool) && _semantics.CurrentValue == BooleanSemanticsKind.ValueRequired)
      {
        string message = string.Format ("Expression type '{0}' was not expected to have boolean type.",  expression.GetType());
        throw new NotSupportedException (message);
      }

      Debug.Assert (
          (_semantics.CurrentValue == BooleanSemanticsKind.ValueRequired && result.Type != typeof (bool))
          || (_semantics.CurrentValue == BooleanSemanticsKind.PredicateRequired && result.Type == typeof (bool)));
      return result;
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (bool))
        return expression;

      Expression expressionAsValue = expression.Value.Equals (true) ? Expression.Constant (1) : Expression.Constant (0);

      return ConvertIntValue(expressionAsValue);
    }

    public Expression VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (bool))
        return expression;

      var expressionAsValue = new SqlColumnExpression (typeof (int), expression.OwningTableAlias, expression.ColumnName);
      return ConvertIntValue (expressionAsValue);
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Type != typeof (bool))
      {
        Debug.Assert (_semantics.CurrentValue == BooleanSemanticsKind.ValueRequired); // ensured by check in VisitExpression
        return expression; // TODO: return base.VisitBinaryExpression (expression);
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
          //case ExpressionType.AndAlso:
          //case ExpressionType.OrElse:
          //var oldSemantics = _needsPredicateSemantics;
          //_needsPredicateSemantics = true;
          //left = VisitExpression (left);
          //right = VisitExpression (right);
          //_needsPredicateSemantics = true;
          //break;
          //case ExpressionType.And:
          //case ExpressionType.Or:
          //case ExpressionType.ExclusiveOr:
          // if (expression.Type == typeof (bool))
          // {
          //   var oldSemantics = _needsPredicateSemantics;
          //   _needsPredicateSemantics = true;
          //   // etc
          // }
          // else: _needsPredicateSemantics = false; etc.
      }

      if (left != expression.Left || right != expression.Right)
        expression = Expression.MakeBinary (expression.NodeType, left, right);

      Debug.Assert (expression.Type == typeof (bool)); // if the expression was boolean before, it must still be a boolean here

      switch (_semantics.CurrentValue)
      {
        case BooleanSemanticsKind.ValueRequired:
          return new SqlCaseExpression (expression, Expression.Constant (1), Expression.Constant (0));
        case BooleanSemanticsKind.PredicateRequired:
          return expression;
        default:
          throw new NotSupportedException ("Invalid enum value?");
      }
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlColumListExpression (SqlColumnListExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return base.VisitUnknownExpression (expression);
    }

    private Expression ConvertIntValue (Expression expressionAsValue)
    {
      switch (_semantics.CurrentValue)
      {
        case BooleanSemanticsKind.ValueRequired:
          return expressionAsValue;
        case BooleanSemanticsKind.PredicateRequired:
          return Expression.Equal (expressionAsValue, Expression.Constant (1));
        default:
          throw new NotSupportedException ("Invalid enum value?");
      }
    }
  }
}