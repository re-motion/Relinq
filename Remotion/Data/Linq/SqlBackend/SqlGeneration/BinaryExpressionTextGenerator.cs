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
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Generates SQL text for <see cref="BinaryExpression"/> instances.
  /// </summary>
  public class BinaryExpressionTextGenerator
  {
    private readonly SqlCommandBuilder _commandBuilder;
    private readonly ExpressionTreeVisitor _expressionVisitor;
    private readonly Dictionary<ExpressionType, string> _simpleOperatorRegistry;

    public BinaryExpressionTextGenerator (SqlCommandBuilder commandBuilder, ExpressionTreeVisitor expressionVisitor)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expressionVisitor", expressionVisitor);

      _commandBuilder = commandBuilder;
      _expressionVisitor = expressionVisitor;

      _simpleOperatorRegistry = new Dictionary<ExpressionType, string>
                                {
                                    { ExpressionType.Add, "+" },           // bool: impossible
                                    { ExpressionType.AddChecked, "+" },    // bool: impossible
                                    { ExpressionType.And, "&" },           // bool: predicate semantics
                                    { ExpressionType.AndAlso, "AND" },     // bool: predicate semantics
                                    { ExpressionType.Divide, "/" },        // bool: impossible
                                    { ExpressionType.ExclusiveOr, "^" },   // bool: predicate semantics
                                    { ExpressionType.GreaterThan, ">" },   // bool: impossible
                                    { ExpressionType.GreaterThanOrEqual, ">=" }, // bool: impossible
                                    { ExpressionType.LessThan, "<" },            // bool: impossible
                                    { ExpressionType.LessThanOrEqual, "<=" },    // bool: impossible
                                    { ExpressionType.Modulo, "%" },              // bool: impossible
                                    { ExpressionType.Multiply, "*" },            // bool: impossible
                                    { ExpressionType.MultiplyChecked, "*" },     // bool: impossible
                                    { ExpressionType.Or, "|" },                  // bool: predicate semantics
                                    { ExpressionType.OrElse, "OR" },             // bool: predicate semantics
                                    { ExpressionType.Subtract, "-" },            // bool: impossible
                                    { ExpressionType.SubtractChecked, "-" }      // bool: impossible
                                };
    }

    public void GenerateSqlForBinaryExpression (BinaryExpression expression)
    {
      //if(expression.NodeType==ExpressionType.AndAlso || expression.NodeType==ExpressionType.OrElse)
      //{
      //  if (IsBool (expression.Left))
      //  {
      //    if (IsBinaryExpression (expression.Right))
      //    {
      //      VisitExpression (expression.Left);
      //      _commandBuilder.Append (string.Format("=1 {0}", expression.NodeType == ExpressionType.AndAlso ? " AND " : " OR "));
      //      VisitExpression (expression.Right);
      //      return expression;
      //    }
      //  }

      //  if (IsBool (expression.Right))
      //  {
      //    if (IsBinaryExpression (expression.Left))
      //    {
      //      VisitExpression (expression.Right);
      //      _commandBuilder.Append (string.Format ("=1 {0}", expression.NodeType == ExpressionType.AndAlso ? " AND " : " OR "));
      //      VisitExpression (expression.Left);
      //      return expression;
      //    }
      //  }
      //}

      //TODO: (IsAdult = 1)
      //TODO: refactor checks
      //TODO: check ExpressionType.OrElse for special cases

      //if (AreBoolConstants (expression.Left, expression.Right))
      //{
      //  switch (expression.NodeType)
      //  {
      //    case ExpressionType.AndAlso:
      //    {
      //      VisitExpression (expression.Left);
      //      _commandBuilder.Append ("=");
      //      VisitExpression (expression.Right);
      //      return expression;
      //    }
      //  }
      //}

      switch (expression.NodeType)
      {
        case ExpressionType.Coalesce:
          GenerateSqlForCoalesce (expression.Left, expression.Right);
          break;
        case ExpressionType.Equal:
          GenerateSqlForNullAwareOperator (expression.Left, expression.Right, "=", "IS NULL");
          break;
        case ExpressionType.NotEqual:
          GenerateSqlForNullAwareOperator (expression.Left, expression.Right, "<>", "IS NOT NULL");
          break;
        default:
          GenerateSqlForSimpleOperator (expression.Left, expression.Right, expression.NodeType);
          break;
      }
    }

    private void GenerateSqlForCoalesce (Expression left, Expression right)
    {
      _commandBuilder.Append ("COALESCE (");
      _expressionVisitor.VisitExpression (left);
      _commandBuilder.Append (", ");
      _expressionVisitor.VisitExpression (right);
      _commandBuilder.Append (")");
    }

    private void GenerateSqlForNullAwareOperator (Expression left, Expression right, string ordinaryOperator, string nullOperator)
    {
      if (IsNullConstant (left))
      {
        _expressionVisitor.VisitExpression (right);
        _commandBuilder.Append (" ");
        _commandBuilder.Append (nullOperator);
      }
      else if (IsNullConstant (right))
      {
        _expressionVisitor.VisitExpression (left);
        _commandBuilder.Append (" ");
        _commandBuilder.Append (nullOperator);
      }
      else
      {
        _expressionVisitor.VisitExpression (left);
        _commandBuilder.Append (" ");
        _commandBuilder.Append (ordinaryOperator);
        _commandBuilder.Append (" ");
        _expressionVisitor.VisitExpression (right);
      }
    }

    private void GenerateSqlForSimpleOperator (Expression left, Expression right, ExpressionType nodeType)
    {
      string operatorString;
      if (!_simpleOperatorRegistry.TryGetValue (nodeType, out operatorString))
        throw new NotSupportedException ("The binary operator '" + nodeType + "' is not supported.");

      _expressionVisitor.VisitExpression (left);
      _commandBuilder.Append (" ");
      _commandBuilder.Append (operatorString);
      _commandBuilder.Append (" ");
      _expressionVisitor.VisitExpression (right);
    }

    private bool IsNullConstant (Expression expression)
    {
      var constantExpression = expression as ConstantExpression;
      if (constantExpression != null)
      {
        if (constantExpression.Value == null)
          return true;
      }
      return false;
    }

    private bool IsBinaryExpression (Expression expression)
    {
      return (expression is BinaryExpression);
    }

    private bool IsBool (Expression expression)
    {
      return (expression.Type == typeof (bool));
    }

    private bool AreBoolConstants (Expression leftExpression, Expression rightExpression)
    {
      if ((leftExpression is ConstantExpression) && (rightExpression is ConstantExpression))
        return (leftExpression.Type == typeof (Boolean)) && (rightExpression.Type == typeof (Boolean));
      return false;
    }
  }
}