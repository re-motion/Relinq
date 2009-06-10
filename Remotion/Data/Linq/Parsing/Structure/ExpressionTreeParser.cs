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
    private readonly UniqueIdentifierGenerator _identifierGenerator = new UniqueIdentifierGenerator ();

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

      // TODO: call SubQueryFindingVisitor
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
        var constantExpression = expression as ConstantExpression;
        if (constantExpression == null)
        {
          var message = string.Format (
              "Cannot parse expression '{0}' as it is of type '{1}'. Only MethodCallExpressions and expressions that can be evaluated to a constant "
              + "query source can be parsed.", 
              expression, 
              expression.NodeType) ;
          throw new ParserException (message);
        }

        try
        {
          return new ConstantExpressionNode (associatedIdentifier, constantExpression.Type, constantExpression.Value);
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
      if (methodCallExpression.Arguments.Count > 1 && methodCallExpression.Arguments[1] is UnaryExpression)
      {
        var operand = ((UnaryExpression) methodCallExpression.Arguments[1]).Operand;
        var lambdaExpression = operand as LambdaExpression;
        if (lambdaExpression != null && lambdaExpression.Parameters.Count == 1)
          return lambdaExpression.Parameters[0].Name;
      }
      return null;
    }
  }
}