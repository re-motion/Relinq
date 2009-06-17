// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

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

    public IExpressionNode Parse (string associatedIdentifier, IExpressionNode source, MethodCallExpression expressionToParse)
    {
      ArgumentUtility.CheckNotNull ("expressionToParse", expressionToParse);

      Type nodeType = GetNodeType(expressionToParse);
      var additionalConstructorParameters = expressionToParse.Arguments
            .Skip (1) // skip the expression corresponding to the source argument
            .Select (expr => ConvertExpressionToParameterValue (expr)) // convert the remaining argument expressions to their actual values
            .ToArray();

      var parseInfo = new MethodCallExpressionParseInfo(associatedIdentifier, source, expressionToParse);
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
            expressionToParse, expressionToParse.Method.DeclaringType.FullName, expressionToParse.Method.Name);
        throw new ParserException (message, ex);
      }
    }

    private object ConvertExpressionToParameterValue (Expression expression)
    {
      // Each argument of a MethodCallExpression will either be a UnaryExpression/Quote, which represents an expression passed to the method,
      // a ConstantExpression that contains the expression passed to the method,
      // or any other expression that represents a constant passed to the method.
      // We only support the former two, to support the latter, PartialTreeEvaluatingVisitor must be used.

      if (expression.NodeType == ExpressionType.Constant)
        return ((ConstantExpression) expression).Value;
      else if (expression.NodeType == ExpressionType.Quote)
        return ((UnaryExpression) expression).Operand;
      else
      {
        var message = string.Format (
            "The parameter expression type '{0}' is not supported by MethodCallExpressionParser. Only UnaryExpressions and ConstantExpressions are "
            + "supported. To transform other expressions to ConstantExpressions, use PartialTreeEvaluatingVisitor to simplify the expression tree.",
            expression.NodeType);
        throw new ParserException (message);
      }
    }

    private IExpressionNode CreateExpressionNode (Type nodeType, MethodCallExpressionParseInfo parseInfo, object[] additionalConstructorParameters)
    {
      return MethodCallExpressionNodeFactory.CreateExpressionNode (nodeType, parseInfo, additionalConstructorParameters);
    }
  }
}