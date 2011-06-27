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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.Structure
{
  /// <summary>
  /// Parses a <see cref="MethodCallExpression"/> and creates an <see cref="IExpressionNode"/> from it. This is used by 
  /// <see cref="ExpressionTreeParser"/> for parsing whole expression trees.
  /// </summary>
  public class MethodCallExpressionParser
  {
    private readonly INodeTypeProvider _nodeTypeProvider;

    public MethodCallExpressionParser (INodeTypeProvider nodeTypeProvider)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeProvider", nodeTypeProvider);
      _nodeTypeProvider = nodeTypeProvider;
    }

    public IExpressionNode Parse (
        string associatedIdentifier, IExpressionNode source, IEnumerable<Expression> arguments, MethodCallExpression expressionToParse)
    {
      ArgumentUtility.CheckNotNull ("expressionToParse", expressionToParse);

      Type nodeType = GetNodeType (expressionToParse);
      var additionalConstructorParameters = arguments.Select (expr => ProcessArgumentExpression (expr)).ToArray();

      var parseInfo = new MethodCallExpressionParseInfo (associatedIdentifier, source, expressionToParse);
      return CreateExpressionNode (nodeType, parseInfo, additionalConstructorParameters);
    }

    private Type GetNodeType (MethodCallExpression expressionToParse)
    {
      var nodeType = _nodeTypeProvider.GetNodeType (expressionToParse.Method);
      if (nodeType == null)
      {
        string message = string.Format (
            "Could not parse expression '{0}': This overload of the method '{1}.{2}' is currently not supported.",
            FormattingExpressionTreeVisitor.Format (expressionToParse),
            expressionToParse.Method.DeclaringType.FullName,
            expressionToParse.Method.Name);
        throw new ParserException (message);
      }
      return nodeType;
    }

    private Expression ProcessArgumentExpression (Expression argumentExpression)
    {
      // First, convert the argument expressions to their actual values - this unwraps ConstantantExpressions and UnaryExpressions
      var convertedParameters = UnwrapArgumentExpression (argumentExpression);
      // Then, detect subqueries
      var parametersWithSubQueriesDetected = SubQueryFindingExpressionTreeVisitor.Process (convertedParameters, _nodeTypeProvider);

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

    private IExpressionNode CreateExpressionNode (
        Type nodeType,
        MethodCallExpressionParseInfo parseInfo,
        object[] additionalConstructorParameters)
    {
      try
      {
        return MethodCallExpressionNodeFactory.CreateExpressionNode (nodeType, parseInfo, additionalConstructorParameters);
      }
      catch (ExpressionNodeInstantiationException ex)
      {
        string message = string.Format (
            "Could not parse expression '{0}': {1}",
            FormattingExpressionTreeVisitor.Format (parseInfo.ParsedExpression),
            ex.Message);
        throw new ParserException (message);
      }
    }
  }
}