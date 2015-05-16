// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation
{
  /// <summary>
  /// Analyzes an expression tree by visiting each of its nodes, finding those subtrees that can be evaluated without modifying the meaning of
  /// the tree.
  /// </summary>
  /// <remarks>
  /// An expression node/subtree is evaluatable if:
  /// <list type="bullet">
  /// <item>it is not a <see cref="ParameterExpression"/> or any non-standard expression, </item>
  /// <item>it is not a <see cref="MethodCallExpression"/> that involves an <see cref="IQueryable"/>, and</item>
  /// <item>it does not have any of those non-evaluatable expressions as its children.</item>
  /// </list>
  /// <para>
  /// <see cref="ParameterExpression"/> nodes are not evaluatable because they usually identify the flow of
  /// some information from one query node to the next. 
  /// </para><para>
  /// <see cref="MethodCallExpression"/> nodes that involve <see cref="IQueryable"/> parameters or object instances are not evaluatable because they 
  /// should usually be translated into the target query syntax.
  /// </para><para>
  /// In .NET 3.5, non-standard expressions are not evaluatable because they cannot be compiled and evaluated by LINQ. 
  /// In .NET 4.0, non-standard expressions can be evaluated if they can be reduced to an evaluatable expression.
  /// </para>
  /// </remarks>
  public sealed class EvaluatableTreeFindingExpressionVisitor : RelinqExpressionVisitor, IPartialEvaluationExceptionExpressionVisitor
  {
    public static PartialEvaluationInfo Analyze (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);

      var visitor = new EvaluatableTreeFindingExpressionVisitor();
      visitor.Visit (expressionTree);
      return visitor._partialEvaluationInfo;
    }

    private readonly PartialEvaluationInfo _partialEvaluationInfo = new PartialEvaluationInfo();
    private bool _isCurrentSubtreeEvaluatable;

    private EvaluatableTreeFindingExpressionVisitor ()
    {
    }

    public override Expression Visit (Expression expression)
    {
      if (expression == null)
        return base.Visit (expression);

      // An expression node/subtree is evaluatable iff:
      // - by itself it would be evaluatable, and
      // - it does not contain any non-evaluatable expressions.

      // To find these nodes, first assume that the current subtree is evaluatable iff it is one of the standard nodes. Store the evaluatability 
      // of the parent node for later.
      bool isParentNodeEvaluatable = _isCurrentSubtreeEvaluatable;
      _isCurrentSubtreeEvaluatable = IsSupportedStandardExpression (expression);

      // Then call the specific Visit... method for this expression. This will determine if this node by itself is not evaluatable by setting 
      // _isCurrentSubtreeEvaluatable to false if it isn't. It will also investigate the evaluatability info of the child nodes and set 
      // _isCurrentSubtreeEvaluatable accordingly.
      var visitedExpression = base.Visit (expression);

      // If the current subtree is still marked to be evaluatable, put it into the result list.
      if (_isCurrentSubtreeEvaluatable)
        _partialEvaluationInfo.AddEvaluatableExpression (expression);

      // Before returning to the parent node, set the evaluatability of the parent node.
      // The parent node can be evaluatable only if (among other things):
      //   - it was evaluatable before, and
      //   - the current subtree (i.e. the child of the parent node) is evaluatable.
      _isCurrentSubtreeEvaluatable &= isParentNodeEvaluatable; // the _isCurrentSubtreeEvaluatable flag now relates to the parent node again
      return visitedExpression;
    }

    protected override Expression VisitParameter (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // Parameters are not evaluatable.
      _isCurrentSubtreeEvaluatable = false;
      return base.VisitParameter (expression);
    }

    protected override Expression VisitMethodCall (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // Method calls are only evaluatable if they do not involve IQueryable objects.

      if (IsQueryableExpression (expression.Object))
        _isCurrentSubtreeEvaluatable = false;

      for (int i = 0; i < expression.Arguments.Count && _isCurrentSubtreeEvaluatable; i++)
      {
        if (IsQueryableExpression (expression.Arguments[i]))
          _isCurrentSubtreeEvaluatable = false;
      }

      return base.VisitMethodCall (expression);
    }

    protected override Expression VisitMember (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // MemberExpressions are only evaluatable if they do not involve IQueryable objects.

      if (IsQueryableExpression (expression.Expression))
        _isCurrentSubtreeEvaluatable = false;

      return base.VisitMember (expression);
    }

    protected override Expression VisitMemberInit (MemberInitExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Visit (expression.Bindings, VisitMemberBinding);

      // Visit the NewExpression only if the List initializers is evaluatable. It makes no sense to evaluate the ListExpression if the initializers
      // cannot be evaluated.

      if (!_isCurrentSubtreeEvaluatable)
        return expression;

      Visit (expression.NewExpression);
      return expression;
    }

    protected override Expression VisitListInit (ListInitExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Visit (expression.Initializers, VisitElementInit);

      // Visit the NewExpression only if the List initializers is evaluatable. It makes no sense to evaluate the NewExpression if the initializers
      // cannot be evaluated.

      if (!_isCurrentSubtreeEvaluatable)
        return expression;

      Visit (expression.NewExpression);
      return expression;
    }

#if NET_3_5
    protected override Expression VisitRelinqUnknownNonExtension (Expression expression)
    {
      //ignore
      return expression;
    }
#endif

#if !NET_3_5
    /// <summary>
    /// Determines whether the given <see cref="Expression"/> is one of the expressions defined by <see cref="ExpressionType"/> for which
    /// <see cref="ExpressionVisitor"/> has a dedicated Visit method. <see cref="ExpressionVisitor.Visit(Expression)"/> handles those by calling the respective Visit method.
    /// </summary>
    /// <param name="expression">The expression to check. Must not be <see langword="null" />.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="expression"/> is one of the expressions defined by <see cref="ExpressionType"/> and 
    /// <see cref="ExpressionVisitor"/> has a dedicated Visit method for it; otherwise, <see langword="false"/>. 
    /// Note that <see cref="ExpressionType.Extension"/>-type expressions are considered 'not supported' and will also return <see langword="false"/>.
    /// </returns>
    private bool IsSupportedStandardExpression (Expression expression)
    {
      if (expression.NodeType == ExpressionType.Extension)
      {
        if (!expression.CanReduce)
          return false;

        var reducedExpression = expression.ReduceAndCheck();
        return IsSupportedStandardExpression (reducedExpression);
      }

      // Note: In .NET 4.0, all expressions supplied by System.Core have a dedicated Visit method provided by System.Linq.Parsing.ExpressionVisitor.
      return Enum.IsDefined (typeof (ExpressionType), expression.NodeType);
    }
#else
    /// <summary>
    /// Determines whether the given <see cref="Expression"/> is one of the expressions defined by <see cref="ExpressionType"/> for which
    /// <see cref="ExpressionVisitor"/> has a Visit method. <see cref="ExpressionVisitor.Visit(Expression)"/> handles those by calling the respective Visit method.
    /// </summary>
    /// <param name="expression">The expression to check. Must not be <see langword="null" />.</param>
    /// <returns>
    /// 	<see langword="true"/> if <paramref name="expression"/> is one of the expressions defined by <see cref="ExpressionType"/> and 
    ///   <see cref="ExpressionVisitor"/> has a Visit method for it; otherwise, <see langword="false"/>.
    /// </returns>
    private bool IsSupportedStandardExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // Note: Do not use Enum.IsDefined here - this method must only return true if we have a dedicated Visit method. In .NET 4.0, additional expression
      // types have been introduced for which the .NET 3.5 implementation of ExpressionVisitor does not have corresponding Visit methods.
      switch (expression.NodeType)
      {
        case ExpressionType.ArrayLength:
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
        case ExpressionType.Not:
        case ExpressionType.Quote:
        case ExpressionType.TypeAs:
        case ExpressionType.UnaryPlus:
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
        case ExpressionType.Divide:
        case ExpressionType.Modulo:
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
        case ExpressionType.Power:
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
        case ExpressionType.And:
        case ExpressionType.Or:
        case ExpressionType.ExclusiveOr:
        case ExpressionType.LeftShift:
        case ExpressionType.RightShift:
        case ExpressionType.AndAlso:
        case ExpressionType.OrElse:
        case ExpressionType.Equal:
        case ExpressionType.NotEqual:
        case ExpressionType.GreaterThanOrEqual:
        case ExpressionType.GreaterThan:
        case ExpressionType.LessThan:
        case ExpressionType.LessThanOrEqual:
        case ExpressionType.Coalesce:
        case ExpressionType.ArrayIndex:
        case ExpressionType.Conditional:
        case ExpressionType.Constant:
        case ExpressionType.Invoke:
        case ExpressionType.Lambda:
        case ExpressionType.MemberAccess:
        case ExpressionType.Call:
        case ExpressionType.New:
        case ExpressionType.NewArrayBounds:
        case ExpressionType.NewArrayInit:
        case ExpressionType.MemberInit:
        case ExpressionType.ListInit:
        case ExpressionType.Parameter:
        case ExpressionType.TypeIs:
          return true;
      }
      return false;
    }
#endif

    private bool IsQueryableExpression (Expression expression)
    {
      return expression != null && typeof (IQueryable).GetTypeInfo().IsAssignableFrom (expression.Type.GetTypeInfo());
    }

    public Expression VisitPartialEvaluationException (PartialEvaluationExceptionExpression partialEvaluationExceptionExpression)
    {
      ArgumentUtility.CheckNotNull ("partialEvaluationExceptionExpression", partialEvaluationExceptionExpression);

      // PartialEvaluationExceptionExpression is not evaluable, and its children aren't either (so we don't visit them).
      _isCurrentSubtreeEvaluatable = false;
      return partialEvaluationExceptionExpression;
    }
  }
}
