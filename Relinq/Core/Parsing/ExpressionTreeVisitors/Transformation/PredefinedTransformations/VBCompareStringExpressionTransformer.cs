// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  /// <summary>
  /// Detects expressions calling the CompareString method used by Visual Basic .NET, and replaces them with 
  /// <see cref="VBStringComparisonExpression"/> instances. Providers use this transformation to be able to handle VB string comparisons
  /// more easily. See <see cref="VBStringComparisonExpression"/> for details.
  /// </summary>
  public class VBCompareStringExpressionTransformer : IExpressionTransformer<BinaryExpression>
  {
    private const string c_vbOperatorsClassName = "Microsoft.VisualBasic.CompilerServices.Operators";
    private const string c_vbCompareStringOperatorMethodName = "CompareString";

    public ExpressionType[] SupportedExpressionTypes
    {
      get
      {
        return new[]
               {
                   ExpressionType.Equal,
                   ExpressionType.NotEqual, 
                   ExpressionType.LessThan,
                   ExpressionType.GreaterThan, 
                   ExpressionType.LessThanOrEqual,
                   ExpressionType.GreaterThanOrEqual
               };
      }
    }
    
    public Expression Transform (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      var leftSideAsMethodCallExpression = expression.Left as MethodCallExpression;
      if (leftSideAsMethodCallExpression != null && (IsVBOperator (leftSideAsMethodCallExpression.Method, c_vbCompareStringOperatorMethodName)))
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

      return expression;
    }

    private Expression GetExpressionForNodeType (
        BinaryExpression expression, MethodCallExpression leftSideAsMethodCallExpression, ConstantExpression leftSideArgument2AsConstantExpression)
    {
      BinaryExpression binaryExpression;
      switch (expression.NodeType)
      {
        case ExpressionType.Equal:
          binaryExpression = Expression.Equal (leftSideAsMethodCallExpression.Arguments[0], leftSideAsMethodCallExpression.Arguments[1]);
          return new VBStringComparisonExpression (binaryExpression, (bool) leftSideArgument2AsConstantExpression.Value);
        case ExpressionType.NotEqual:
          binaryExpression = Expression.NotEqual (leftSideAsMethodCallExpression.Arguments[0], leftSideAsMethodCallExpression.Arguments[1]);
          return new VBStringComparisonExpression (binaryExpression, (bool) leftSideArgument2AsConstantExpression.Value);
      }

      var methodCallExpression = MethodCallExpression.Call (
          leftSideAsMethodCallExpression.Arguments[0],
          typeof (string).GetMethod ("CompareTo", new[] { typeof (string) }),
          leftSideAsMethodCallExpression.Arguments[1]);
      var vbExpression = new VBStringComparisonExpression (methodCallExpression, (bool) leftSideArgument2AsConstantExpression.Value);

      if (expression.NodeType == ExpressionType.GreaterThan)
        return Expression.GreaterThan (vbExpression, Expression.Constant (0));
      else if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
        return Expression.GreaterThanOrEqual (vbExpression, Expression.Constant (0));
      else if (expression.NodeType == ExpressionType.LessThan)
        return Expression.LessThan (vbExpression, Expression.Constant (0));
      else if (expression.NodeType == ExpressionType.LessThanOrEqual)
        return Expression.LessThanOrEqual (vbExpression, Expression.Constant (0));

      throw new NotSupportedException (
          string.Format ("Binary expression with node type '{0}' is not supported in a VB string comparison.", expression.NodeType));
    }

    private bool IsVBOperator (MethodInfo operatorMethod, string operatorName)
    {
      return operatorMethod.DeclaringType.FullName == c_vbOperatorsClassName && operatorMethod.Name == operatorName;
    }
  }
}