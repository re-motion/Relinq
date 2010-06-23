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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Parses a <see cref="MethodCallExpression"/> and creates an <see cref="IExpressionNode"/> from it. This is used by 
  /// <see cref="ExpressionTreeParser"/> for parsing whole expression trees.
  /// </summary>
  public class MethodCallExpressionParser
  {
    private readonly MethodCallExpressionNodeTypeRegistry _nodeTypeRegistry;

    public MethodCallExpressionParser (MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);
      _nodeTypeRegistry = nodeTypeRegistry;
    }

    public IExpressionNode Parse (string associatedIdentifier, IExpressionNode source, IEnumerable<Expression> arguments, MethodCallExpression expressionToParse)
    {
      ArgumentUtility.CheckNotNull ("expressionToParse", expressionToParse);

      Type nodeType = GetNodeType (expressionToParse);
      var additionalConstructorParameters = arguments.Select (expr => ProcessArgumentExpression(expr)).ToArray();

      var parseInfo = new MethodCallExpressionParseInfo (associatedIdentifier, source, expressionToParse);
      return CreateExpressionNode (nodeType, parseInfo, additionalConstructorParameters);
    }

    private Type GetNodeType (MethodCallExpression expressionToParse)
    {
      try
      {
        return _nodeTypeRegistry.GetNodeType (expressionToParse.Method);
      }
      catch (KeyNotFoundException ex)
      {
        string message = string.Format (
            "Could not parse expression '{0}': This overload of the method '{1}.{2}' is currently not supported, but you can register your own parser if needed.",
            expressionToParse,
            expressionToParse.Method.DeclaringType.FullName,
            expressionToParse.Method.Name);
        throw new ParserException (message, ex);
      }
    }

    private Expression ProcessArgumentExpression (Expression argumentExpression)
    {
      // First, convert the argument expressions to their actual values - this unwraps ConstantantExpressions and UnaryExpressions
      var convertedParameters = UnwrapArgumentExpression (argumentExpression);
      // Then, detect subqueries
      var parametersWithSubQueriesDetected = PreprocessingExpressionTreeVisitor.Process (convertedParameters, _nodeTypeRegistry);

      return parametersWithSubQueriesDetected;
    }


    private Expression UnwrapArgumentExpression (Expression expression)
    {
      // Each argument of a MethodCallExpression will either be a UnaryExpression/Quote, which represents an expression passed to a Queryable method,
      // a LambdaExpression, which represents an expression passed to an Enumerable method,
      // a ConstantExpression that contains the expression passed to the method,
      // or any other expression that represents a constant passed to the method.
      // We only support the former three, to support the latter, PartialEvaluatingExpressionTreeVisitor must be used.

      if (expression.NodeType == ExpressionType.Quote)
        return ((UnaryExpression) expression).Operand;
      else if (expression.NodeType == ExpressionType.Constant && ((ConstantExpression) expression).Value is LambdaExpression)
        return (Expression) ((ConstantExpression) expression).Value;
      else
        return expression;
    }

    private IExpressionNode CreateExpressionNode (Type nodeType, MethodCallExpressionParseInfo parseInfo, object[] additionalConstructorParameters)
    {
      return MethodCallExpressionNodeFactory.CreateExpressionNode (nodeType, parseInfo, additionalConstructorParameters);
    }
  }
}
