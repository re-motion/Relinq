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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
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
                                    { ExpressionType.Add, "+" },           
                                    { ExpressionType.AddChecked, "+" },    
                                    { ExpressionType.And, "&" },           
                                    { ExpressionType.AndAlso, "AND" },     
                                    { ExpressionType.Divide, "/" },        
                                    { ExpressionType.ExclusiveOr, "^" },   
                                    { ExpressionType.GreaterThan, ">" },   
                                    { ExpressionType.GreaterThanOrEqual, ">=" }, 
                                    { ExpressionType.LessThan, "<" },            
                                    { ExpressionType.LessThanOrEqual, "<=" },    
                                    { ExpressionType.Modulo, "%" },              
                                    { ExpressionType.Multiply, "*" },            
                                    { ExpressionType.MultiplyChecked, "*" },     
                                    { ExpressionType.Or, "|" },                  
                                    { ExpressionType.OrElse, "OR" },             
                                    { ExpressionType.Subtract, "-" },            
                                    { ExpressionType.SubtractChecked, "-" },     
                                    { ExpressionType.Coalesce, "COALESCE" },            
                                    { ExpressionType.Power, "POWER" }                
                                };
    }

    public void GenerateSqlForBinaryExpression (BinaryExpression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.Equal:
          GenerateSqlForEqualityOperator (expression.Left, expression.Right, "=", "IS NULL");
          break;
        case ExpressionType.NotEqual:
          GenerateSqlForEqualityOperator (expression.Left, expression.Right, "<>", "IS NOT NULL");
          break;
        case ExpressionType.Coalesce:
        case ExpressionType.Power:
          GenerateSqlForPrefixOperator (expression.Left, expression.Right, expression.NodeType);
          break;
        default:
          GenerateSqlForInfixOperator (expression.Left, expression.Right, expression.NodeType);
          break;
      }
    }

    private void GenerateSqlForEqualityOperator (Expression left, Expression right, string ordinaryOperator, string nullOperator)
    {
      if (IsNullConstant (left))
      {
        VisitEqualsOperand(right);
        _commandBuilder.Append (" ");
        _commandBuilder.Append (nullOperator);
      }
      else if (IsNullConstant (right))
      {
        VisitEqualsOperand (left);
        _commandBuilder.Append (" ");
        _commandBuilder.Append (nullOperator);
      }
      else
      {
        VisitEqualsOperand (left);
        _commandBuilder.Append (" ");
        _commandBuilder.Append (ordinaryOperator);
        _commandBuilder.Append (" ");
        VisitEqualsOperand (right);
      }
    }

    private void VisitEqualsOperand (Expression expression)
    {
      if (expression is SqlEntityExpression)
        _expressionVisitor.VisitExpression (((SqlEntityExpression) expression).PrimaryKeyColumn);
      else
        _expressionVisitor.VisitExpression (expression);
    }

    private void GenerateSqlForPrefixOperator (Expression left, Expression right, ExpressionType nodeType)
    {
      string operatorString = GetRegisteredOperatorString (nodeType);
      _commandBuilder.Append (operatorString);
      _commandBuilder.Append (" (");
      _expressionVisitor.VisitExpression (left);
      _commandBuilder.Append (", ");
      _expressionVisitor.VisitExpression (right);
      _commandBuilder.Append (")");
    }

    private void GenerateSqlForInfixOperator (Expression left, Expression right, ExpressionType nodeType)
    {
      string operatorString = GetRegisteredOperatorString(nodeType);

      _expressionVisitor.VisitExpression (left);
      _commandBuilder.Append (" ");
      _commandBuilder.Append (operatorString);
      _commandBuilder.Append (" ");
      _expressionVisitor.VisitExpression (right);
    }

    private string GetRegisteredOperatorString (ExpressionType nodeType)
    {
      string operatorString;
      if (!_simpleOperatorRegistry.TryGetValue (nodeType, out operatorString))
        throw new NotSupportedException ("The binary operator '" + nodeType + "' is not supported.");
      return operatorString;
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
  }
}