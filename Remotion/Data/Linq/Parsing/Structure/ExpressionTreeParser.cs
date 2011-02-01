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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;

using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Data.Linq.Parsing.Structure.ExpressionTreeProcessingSteps;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.Utilities;
using System.Reflection;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Parses an expression tree into a chain of <see cref="IExpressionNode"/> objects after executing a sequence of 
  /// <see cref="IExpressionTreeProcessingStep"/> objects.
  /// </summary>
  public class ExpressionTreeParser
  {
    private static readonly MethodInfo s_getArrayLengthMethod = typeof (Array).GetMethod ("get_Length");

    public static ExpressionTreeParser CreateDefault ()
    {
      return new ExpressionTreeParser (
          MethodCallExpressionNodeTypeRegistry.CreateDefault (), 
          CreateDefaultProcessingSteps (ExpressionTransformerRegistry.CreateDefault()));
    }

    public static IExpressionTreeProcessingStep[] CreateDefaultProcessingSteps (IExpressionTranformationProvider tranformationProvider)
    {
      ArgumentUtility.CheckNotNull ("tranformationProvider", tranformationProvider);

      return new IExpressionTreeProcessingStep[] { 
          new PartialEvaluationStep(), 
          new ExpressionTransformationStep (tranformationProvider) };
    }

    private readonly UniqueIdentifierGenerator _identifierGenerator = new UniqueIdentifierGenerator ();
    private readonly IMethodCallExpressionNodeTypeProvider _nodeTypeProvider;
    private readonly IExpressionTreeProcessingStep[] _processingSteps;
    private readonly MethodCallExpressionParser _methodCallExpressionParser;

    public ExpressionTreeParser (IMethodCallExpressionNodeTypeProvider nodeTypeProvider, IExpressionTreeProcessingStep[] processingSteps)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeProvider", nodeTypeProvider);
      ArgumentUtility.CheckNotNull ("processingSteps", processingSteps);
      
      _nodeTypeProvider = nodeTypeProvider;
      _processingSteps = processingSteps;
      _methodCallExpressionParser = new MethodCallExpressionParser (_nodeTypeProvider);
    }

    /// <summary>
    /// Gets the node type provider used to parse <see cref="MethodCallExpression"/> instances in <see cref="ParseTree"/>.
    /// </summary>
    /// <value>The node type provider.</value>
    public IMethodCallExpressionNodeTypeProvider NodeTypeProvider
    {
      get { return _nodeTypeProvider; }
    }

    /// <summary>
    /// Gets the processing steps used by <see cref="ParseTree"/> to process the <see cref="Expression"/> tree before analyzing its structure.
    /// </summary>
    /// <value>The processing steps.</value>
    public ReadOnlyCollection<IExpressionTreeProcessingStep> ProcessingSteps
    {
      get { return Array.AsReadOnly (_processingSteps); }
    }

    /// <summary>
    /// Parses the given <paramref name="expressionTree"/> into a chain of <see cref="IExpressionNode"/> instances, using 
    /// <see cref="MethodCallExpressionNodeTypeRegistry"/> to convert expressions to nodes.
    /// </summary>
    /// <param name="expressionTree">The expression tree to parse.</param>
    /// <returns>A chain of <see cref="IExpressionNode"/> instances representing the <paramref name="expressionTree"/>.</returns>
    public IExpressionNode ParseTree (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);

      if (expressionTree.Type == typeof (void))
        throw new ParserException (String.Format ("Expressions of type void ('{0}') are not supported.", expressionTree));

      var processedExpressionTree = _processingSteps.Aggregate (expressionTree, (e, s) => s.Process (e));
      return ParseNode (processedExpressionTree, null);
    }

    /// <summary>
    /// Gets the query operator <see cref="MethodCallExpression"/> represented by <paramref name="expression"/>. If <paramref name="expression"/>
    /// is already a <see cref="MethodCallExpression"/>, that is the assumed query operator. If <paramref name="expression"/> is a 
    /// <see cref="MemberExpression"/> and the member's getter is registered with <see cref="NodeTypeProvider"/>, a corresponding 
    /// <see cref="MethodCallExpression"/> is constructed and returned. Otherwise, <see langword="null" /> is returned.
    /// </summary>
    /// <param name="expression">The expression to get a query operator expression for.</param>
    /// <returns>A <see cref="MethodCallExpression"/> to be parsed as a query operator, or <see langword="null"/> if the expression does not represent
    /// a query operator.</returns>
    public MethodCallExpression GetQueryOperatorExpression (Expression expression)
    {
      var methodCallExpression = expression as MethodCallExpression;
      if (methodCallExpression != null)
        return methodCallExpression;

      var memberExpression = expression as MemberExpression;
      if (memberExpression != null)
      {
        var propertyInfo = memberExpression.Member as PropertyInfo;
        if (propertyInfo == null)
          return null;

        var getterMethod = propertyInfo.GetGetMethod ();
        if (getterMethod == null || !_nodeTypeProvider.IsRegistered (getterMethod))
          return null;

        return Expression.Call (memberExpression.Expression, getterMethod);
      }

      var unaryExpression = expression as UnaryExpression;
      if (unaryExpression != null)
      {
        if (unaryExpression.NodeType == ExpressionType.ArrayLength && _nodeTypeProvider.IsRegistered (s_getArrayLengthMethod))
          return Expression.Call (unaryExpression.Operand, s_getArrayLengthMethod);
      }

      return null;
    }

    private IExpressionNode ParseNode (Expression expression, string associatedIdentifier)
    {
      if (associatedIdentifier == null)
        associatedIdentifier = _identifierGenerator.GetUniqueIdentifier ("<generated>_");

      var methodCallExpression = GetQueryOperatorExpression(expression);
      if (methodCallExpression != null)
        return ParseMethodCallExpression (methodCallExpression, associatedIdentifier);
      else
        return ParseNonQueryOperatorExpression(expression, associatedIdentifier);
    }

    private IExpressionNode ParseMethodCallExpression (MethodCallExpression methodCallExpression, string associatedIdentifier)
    {
      string associatedIdentifierForSource = InferAssociatedIdentifierForSource (methodCallExpression);

      Expression sourceExpression;
      IEnumerable<Expression> arguments;
      if (methodCallExpression.Object != null)
      {
        sourceExpression = methodCallExpression.Object;
        arguments = methodCallExpression.Arguments;
      }
      else
      {
        sourceExpression = methodCallExpression.Arguments[0];
        arguments = methodCallExpression.Arguments.Skip (1);
      }

      var source = ParseNode (sourceExpression, associatedIdentifierForSource);
      return _methodCallExpressionParser.Parse (associatedIdentifier, source, arguments, methodCallExpression);
    }

    private IExpressionNode ParseNonQueryOperatorExpression (Expression expression, string associatedIdentifier)
    {
      var preprocessedExpression = SubQueryFindingExpressionTreeVisitor.Process (expression, _nodeTypeProvider);

      try
      {
        return new MainSourceExpressionNode (associatedIdentifier, preprocessedExpression);
      }
      catch (ArgumentTypeException ex)
      {
        var message = String.Format (
            "Cannot parse expression '{0}' as it has an unsupported type. Only query sources (that is, expressions that implement IEnumerable) "
            + "and query operators can be parsed.",
            preprocessedExpression);
        throw new ParserException (message, ex);
      }
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
      return methodCallExpression.Arguments
          .Select (argument => GetLambdaExpression (argument))
          .FirstOrDefault (lambdaExpression => lambdaExpression != null);
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
