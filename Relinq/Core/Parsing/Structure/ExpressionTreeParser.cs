// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Reflection;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.Structure
{
  /// <summary>
  /// Parses an expression tree into a chain of <see cref="IExpressionNode"/> objects after executing a sequence of 
  /// <see cref="IExpressionTreeProcessor"/> objects.
  /// </summary>
  public class ExpressionTreeParser
  {
    private static readonly MethodInfo s_getArrayLengthMethod = typeof (Array).GetMethod ("get_Length");

    [Obsolete (
        "This method has been removed. Use QueryParser.CreateDefault, or create a customized ExpressionTreeParser using the constructor. (1.13.93)", 
        true)]
    public static ExpressionTreeParser CreateDefault ()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a default <see cref="CompoundNodeTypeProvider"/> that already has all expression node parser defined by the re-linq assembly 
    /// registered. Users can add inner providers to register their own expression node parsers.
    /// </summary>
    /// <returns>A default <see cref="CompoundNodeTypeProvider"/> that already has all expression node parser defined by the re-linq assembly 
    /// registered.</returns>
    public static CompoundNodeTypeProvider CreateDefaultNodeTypeProvider ()
    {
      var searchedTypes = typeof (MethodInfoBasedNodeTypeRegistry).Assembly.GetTypes ();
      var innerProviders = new INodeTypeProvider[]
                           {
                               MethodInfoBasedNodeTypeRegistry.CreateFromTypes (searchedTypes),
                               MethodNameBasedNodeTypeRegistry.CreateFromTypes(searchedTypes)
                           };
      return new CompoundNodeTypeProvider (innerProviders);
    }

    /// <summary>
    /// Creates a default <see cref="CompoundExpressionTreeProcessor"/> that already has the expression tree processing steps defined by the re-linq assembly
    /// registered. Users can insert additional processing steps.
    /// </summary>
    /// <param name="tranformationProvider">The tranformation provider to be used by the <see cref="TransformingExpressionTreeProcessor"/> included
    /// in the result set. Use <see cref="ExpressionTransformerRegistry.CreateDefault"/> to create a default provider.</param>
    /// <returns>
    /// A default <see cref="CompoundExpressionTreeProcessor"/> that already has all expression tree processing steps defined by the re-linq assembly
    /// registered.
    /// </returns>
    /// <remarks>
    /// The following steps are included:
    /// <list type="bullet">
    /// 		<item><see cref="PartialEvaluatingExpressionTreeProcessor"/></item>
    /// 		<item><see cref="TransformingExpressionTreeProcessor"/> (parameterized with <paramref name="tranformationProvider"/>)</item>
    /// 	</list>
    /// </remarks>
    public static CompoundExpressionTreeProcessor CreateDefaultProcessor (IExpressionTranformationProvider tranformationProvider)
    {
      ArgumentUtility.CheckNotNull ("tranformationProvider", tranformationProvider);

      return new CompoundExpressionTreeProcessor (new IExpressionTreeProcessor[] { 
          new PartialEvaluatingExpressionTreeProcessor(), 
          new TransformingExpressionTreeProcessor (tranformationProvider) });
    }

    private readonly UniqueIdentifierGenerator _identifierGenerator = new UniqueIdentifierGenerator ();
    private readonly INodeTypeProvider _nodeTypeProvider;
    private readonly IExpressionTreeProcessor _processor;
    private readonly MethodCallExpressionParser _methodCallExpressionParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionTreeParser"/> class with a custom <see cref="INodeTypeProvider"/> and 
    /// <see cref="IExpressionTreeProcessor"/> implementation.
    /// </summary>
    /// <param name="nodeTypeProvider">The <see cref="INodeTypeProvider"/> to use when parsing <see cref="Expression"/> trees. Use 
    /// <see cref="CreateDefaultNodeTypeProvider"/> to create an instance of <see cref="CompoundNodeTypeProvider"/> that already includes all
    /// default node types. (The <see cref="CompoundNodeTypeProvider"/> can be customized as needed by adding or removing 
    /// <see cref="CompoundNodeTypeProvider.InnerProviders"/>).</param>
    /// <param name="processor">The <see cref="IExpressionTreeProcessor"/> to apply to <see cref="Expression"/> trees before parsing their nodes. Use
    /// <see cref="CreateDefaultProcessor"/> to create an instance of <see cref="CompoundExpressionTreeProcessor"/> that already includes
    /// the default steps. (The <see cref="CompoundExpressionTreeProcessor"/> can be customized as needed by adding or removing 
    /// <see cref="CompoundExpressionTreeProcessor.InnerProcessors"/>).</param>
    public ExpressionTreeParser (INodeTypeProvider nodeTypeProvider, IExpressionTreeProcessor processor)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeProvider", nodeTypeProvider);
      ArgumentUtility.CheckNotNull ("processor", processor);
      
      _nodeTypeProvider = nodeTypeProvider;
      _processor = processor;
      _methodCallExpressionParser = new MethodCallExpressionParser (_nodeTypeProvider);
    }

    /// <summary>
    /// Gets the node type provider used to parse <see cref="MethodCallExpression"/> instances in <see cref="ParseTree"/>.
    /// </summary>
    /// <value>The node type provider.</value>
    public INodeTypeProvider NodeTypeProvider
    {
      get { return _nodeTypeProvider; }
    }

    /// <summary>
    /// Gets the processing steps used by <see cref="ParseTree"/> to process the <see cref="Expression"/> tree before analyzing its structure.
    /// </summary>
    /// <value>The processing steps.</value>
    public IExpressionTreeProcessor Processor
    {
      get { return _processor; }
    }

    /// <summary>
    /// Parses the given <paramref name="expressionTree"/> into a chain of <see cref="IExpressionNode"/> instances, using 
    /// <see cref="MethodInfoBasedNodeTypeRegistry"/> to convert expressions to nodes.
    /// </summary>
    /// <param name="expressionTree">The expression tree to parse.</param>
    /// <returns>A chain of <see cref="IExpressionNode"/> instances representing the <paramref name="expressionTree"/>.</returns>
    public IExpressionNode ParseTree (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);

      if (expressionTree.Type == typeof (void))
        throw new ParserException (String.Format ("Expressions of type void ('{0}') are not supported.", expressionTree));

      var processedExpressionTree = _processor.Process (expressionTree);
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
      if (string.IsNullOrEmpty (associatedIdentifier))
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
