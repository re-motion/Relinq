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
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
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

    public IExpressionNode Parse (IExpressionNode source, MethodCallExpression expressionToParse)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("expressionToParse", expressionToParse);

      Type nodeType = GetNodeType(expressionToParse);
      var additionalConstructorParameters = expressionToParse.Arguments
            .Skip (1) // skip the expression corresponding to the source argument
            .Select (expr => ConvertExpressionToParameterValue (expr)) // convert the remaining argument expressions to their actual values
            .ToArray();
      return CreateExpressionNode(nodeType, source, additionalConstructorParameters);
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
      // or any other expression that represents a constant passed to the method.
      // The partial evaluator will convert Quote expressions into ConstantExpressions holding the actual Expression to pass in and
      // all other expressions into ConstantExpressions holding the value to pass in.
      var evaluatedExpression = PartialTreeEvaluatingVisitor.EvaluateSubtree (expression);
      return evaluatedExpression.Value;
    }

    private IExpressionNode CreateExpressionNode (Type nodeType, IExpressionNode source, object[] additionalConstructorParameters)
    {
      return ExpressionNodeFactory.CreateExpressionNode (nodeType, source, additionalConstructorParameters);
    }
  }
}