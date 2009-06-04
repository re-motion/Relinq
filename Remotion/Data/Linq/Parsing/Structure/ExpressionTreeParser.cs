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
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Parses an expression tree into a chain of <see cref="IExpressionNode"/> objects, partially evaluating expressions and finding subqueries in the 
  /// process.
  /// </summary>
  public class ExpressionTreeParser
  {
    private readonly MethodCallExpressionNodeTypeRegistry _nodeTypeRegistry;

    public ExpressionTreeParser (MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);
      _nodeTypeRegistry = nodeTypeRegistry;
    }

    public MethodCallExpressionNodeTypeRegistry NodeTypeRegistry
    {
      get { return _nodeTypeRegistry; }
    }

    // TODO: call partial tree evaluator and SubQueryFindingVisitor

    public IExpressionNode Parse (Expression expression) // string associatedIdentifier
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // if (associatedIdentifier == null) associatedIdentifier = generate();

      var methodCallExpression = expression as MethodCallExpression;
      if (methodCallExpression != null)
        return ParseMethodCallExpression (methodCallExpression); // , associatedIdentifier
      else
      {
        var constantExpression = PartialTreeEvaluatingVisitor.EvaluateSubtree (expression);
        try
        {
          return new ConstantExpressionNode ("TODO", constantExpression.Type, constantExpression.Value); // associatedIdentifier
        }
        catch (ArgumentTypeException ex)
        {
          var message = string.Format ("Cannot parse expression '{0}' as it has an unsupported type. {1}", expression, ex.Message);
          throw new ParserException (message, ex);
        }
      }
    }

    private IExpressionNode ParseMethodCallExpression (MethodCallExpression methodCallExpression) // string associatedIdentifier
    {
      var parser = new MethodCallExpressionParser (_nodeTypeRegistry);
      // var associatedIdentifierForSource = methodCallExpression.Arguments[1].Operand.Parameters[0].Name / null

      if (methodCallExpression.Arguments.Count == 0)
      {
        var message = string.Format (
            "Cannot parse expression '{0}' because it calls the unsupported method '{1}'. Only query methods "
            + "whose first parameter represents the remaining query chain are supported.",
            methodCallExpression,
            methodCallExpression.Method.Name);
        throw new ParserException (message);
      }

      var source = Parse (methodCallExpression.Arguments[0]); //, associatedIdentifierForSource
      return parser.Parse ("TODO", source, methodCallExpression); // , associatedIdentifier
    }
  }
}