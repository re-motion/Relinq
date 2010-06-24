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
using System.Reflection;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors
{
  /// <summary>
  /// Preprocesses an expression tree for parsing. The preprocessing involves detection of sub-queries and VB-specific expressions.
  /// </summary>
  public class PreprocessingExpressionTreeVisitor : ExpressionTreeVisitor
  {
    public static Expression Process (Expression expressionTree, MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);

      var visitor = new PreprocessingExpressionTreeVisitor (nodeTypeRegistry);
      return visitor.VisitExpression (expressionTree);
    }

    private readonly MethodCallExpressionNodeTypeRegistry _nodeTypeRegistry;
    private readonly QueryParser _innerParser;

    private PreprocessingExpressionTreeVisitor (MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);

      _nodeTypeRegistry = nodeTypeRegistry;
      _innerParser = new QueryParser (new ExpressionTreeParser (_nodeTypeRegistry));
    }

    public override Expression VisitExpression (Expression expression)
    {
      var potentialQueryOperatorExpression = _innerParser.ExpressionTreeParser.GetQueryOperatorExpression (expression);
      if (potentialQueryOperatorExpression != null
          && _innerParser.ExpressionTreeParser.NodeTypeRegistry.IsRegistered (potentialQueryOperatorExpression.Method))
        return CreateSubQueryNode (potentialQueryOperatorExpression);
      else
        return base.VisitExpression (expression);
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      var leftSideAsMethodCallExpression = expression.Left as MethodCallExpression;
      if (leftSideAsMethodCallExpression != null && (IsVBOperator (leftSideAsMethodCallExpression.Method, "CompareString")))
      {
        var rightSideAsConstantExpression = expression.Right as ConstantExpression;
        Debug.Assert (
            rightSideAsConstantExpression != null && rightSideAsConstantExpression.Value is Int32 && (int) rightSideAsConstantExpression.Value == 0,
            "The right side of the binary expression has to be a constant expression with value 0.");

        var leftSideArgument2AsConstantExpression = leftSideAsMethodCallExpression.Arguments[2] as ConstantExpression;
        Debug.Assert (
            leftSideArgument2AsConstantExpression != null && leftSideArgument2AsConstantExpression.Value is bool,
            "The second argument of the method call expression has to be a constant expression with a boolean value.");

       return GetExpressionForNodeType (expression, leftSideAsMethodCallExpression, leftSideArgument2AsConstantExpression);
      }
      return base.VisitBinaryExpression (expression);
    }

    private Expression GetExpressionForNodeType (BinaryExpression expression, MethodCallExpression leftSideAsMethodCallExpression, ConstantExpression leftSideArgument2AsConstantExpression)
    {
      BinaryExpression binaryExpression;
      switch (expression.NodeType)
      {
        case ExpressionType.Equal:
          binaryExpression = Expression.Equal (leftSideAsMethodCallExpression.Arguments[0], leftSideAsMethodCallExpression.Arguments[1]);
          return new VBStringComparisonExpression (typeof (bool), binaryExpression, (bool) leftSideArgument2AsConstantExpression.Value);
        case ExpressionType.NotEqual:
          binaryExpression = Expression.NotEqual (leftSideAsMethodCallExpression.Arguments[0], leftSideAsMethodCallExpression.Arguments[1]);
          return new VBStringComparisonExpression (typeof (bool), binaryExpression, (bool) leftSideArgument2AsConstantExpression.Value);
      }

      var methodCallExpression = MethodCallExpression.Call (
              leftSideAsMethodCallExpression.Arguments[0],
              typeof(string).GetMethod("CompareTo", new[] { typeof(string) }),
              leftSideAsMethodCallExpression.Arguments[1]);
          var vbExpression = new VBStringComparisonExpression (typeof (int), methodCallExpression, (bool) leftSideArgument2AsConstantExpression.Value);

      if(expression.NodeType==ExpressionType.GreaterThan)
        return Expression.GreaterThan (vbExpression, Expression.Constant(0));
      else if(expression.NodeType==ExpressionType.GreaterThanOrEqual)
        return Expression.GreaterThanOrEqual (vbExpression, Expression.Constant (0));
      else if(expression.NodeType==ExpressionType.LessThan)
        return Expression.LessThan(vbExpression, Expression.Constant (0));
      else if(expression.NodeType==ExpressionType.LessThanOrEqual)
        return Expression.LessThanOrEqual (vbExpression, Expression.Constant (0));

      throw new NotSupportedException (string.Format ("Binary expression with node type '{0}' is not supported.", expression.NodeType));
    }

    protected internal override Expression VisitUnknownExpression (Expression expression)
    {
      //ignore
      return expression;
    }

    private SubQueryExpression CreateSubQueryNode (MethodCallExpression methodCallExpression)
    {
      QueryModel queryModel = _innerParser.GetParsedQuery (methodCallExpression);
      return new SubQueryExpression (queryModel);
    }

    private bool IsVBOperator (MethodInfo operatorMethod, string operatorName)
    {
      return operatorMethod.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators" && operatorMethod.Name == operatorName;
    }
  }
}