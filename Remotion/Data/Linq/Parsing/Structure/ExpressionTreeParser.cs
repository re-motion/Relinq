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
  /// Parses an expression tree into a chain of <see cref="IExpressionNode"/> objects, partially evaluating expressions in the process.
  /// </summary>
  public class ExpressionTreeParser
  {
    private readonly MethodCallExpressionNodeTypeRegistry _nodeTypeRegistry;
    private readonly UniqueIdentifierGenerator _identifierGenerator = new UniqueIdentifierGenerator();

    public ExpressionTreeParser (MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);
      _nodeTypeRegistry = nodeTypeRegistry;
    }

    public MethodCallExpressionNodeTypeRegistry NodeTypeRegistry
    {
      get { return _nodeTypeRegistry; }
    }


    public IExpressionNode ParseTree (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);

      if (expressionTree.Type == typeof (void))
        throw new ParserException (string.Format ("Expressions of type void ('{0}') are not supported.", expressionTree));

      var simplifiedExpressionTree = PartialTreeEvaluatingVisitor.EvaluateIndependentSubtrees (expressionTree);
      return ParseNode (simplifiedExpressionTree, null);
    }

    private IExpressionNode ParseNode (Expression expression, string associatedIdentifier)
    {
      if (associatedIdentifier == null)
        associatedIdentifier = _identifierGenerator.GetUniqueIdentifier ("<generated>_");

      var methodCallExpression = expression as MethodCallExpression;
      if (methodCallExpression != null)
        return ParseMethodCallExpression (methodCallExpression, associatedIdentifier);
      else
      {
        try
        {
          return new MainSourceExpressionNode (associatedIdentifier, expression);
        }
        catch (ArgumentTypeException ex)
        {
          var message = string.Format (
              "Cannot parse expression '{0}' as it has an unsupported type. Only query sources (that is, expressions that implement IEnumerable) "
              + "can be parsed.",
              expression);
          throw new ParserException (message, ex);
        }
      }
    }

    private IExpressionNode ParseMethodCallExpression (MethodCallExpression methodCallExpression, string associatedIdentifier)
    {
      if (methodCallExpression.Arguments.Count == 0)
      {
        var message = string.Format (
            "Cannot parse expression '{0}' because it calls the unsupported method '{1}'. Only query methods "
            + "whose first parameter represents the remaining query chain are supported.",
            methodCallExpression,
            methodCallExpression.Method.Name);
        throw new ParserException (message);
      }

      string associatedIdentifierForSource = InferAssociatedIdentifierForSource (methodCallExpression);
      var source = ParseNode (methodCallExpression.Arguments[0], associatedIdentifierForSource);

      var parser = new MethodCallExpressionParser (_nodeTypeRegistry);
      return parser.Parse (associatedIdentifier, source, methodCallExpression);
    }

    /// <summary>
    /// Infers the associated identifier for the source expression node contained in methodCallExpression.Arguments[0]. For example, for the
    /// call chain "<c>source.Where (i => i > 5)</c>" (which actually reads "<c>Where (source, i => i > 5</c>"), the identifier "i" is associated
    /// with the node generated for "source". If no identifier can be inferred, <see langword="null"/> is returned.
    /// </summary>
    private string InferAssociatedIdentifierForSource (MethodCallExpression methodCallExpression)
    {
      var lambdaExpression = GetLambdaArgument (methodCallExpression);
      if (lambdaExpression != null && lambdaExpression.Parameters.Count == 1)
          return lambdaExpression.Parameters[0].Name;
      else
        return null;
    }

    private LambdaExpression GetLambdaArgument (MethodCallExpression methodCallExpression)
    {
      foreach (var argument in methodCallExpression.Arguments)
      {
        var lambdaExpression = GetLambdaExpression(argument);
        if (lambdaExpression != null)
          return lambdaExpression;
      }
      return null;
    }

    private LambdaExpression GetLambdaExpression (Expression expression)
    {
      var lambdaExpression = expression as LambdaExpression;
      if (lambdaExpression != null)
        return lambdaExpression;
      else
      {
        var unaryExpression = expression as UnaryExpression;
        if (unaryExpression != null)
          return unaryExpression.Operand as LambdaExpression;
        else
          return null;
      }
    }
  }
}