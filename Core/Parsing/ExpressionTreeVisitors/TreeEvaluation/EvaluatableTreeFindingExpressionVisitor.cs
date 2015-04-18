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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.TreeEvaluation
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
  /// <see cref="ParameterExpression"/> nodes are not evaluatable because they usually identify the flow of
  /// some information from one query node to the next. 
  /// <see cref="MethodCallExpression"/> nodes that involve <see cref="IQueryable"/> parameters or object instances are not evaluatable because they 
  /// should usually be translated into the target query syntax.
  /// Non-standard expressions are not evaluatable because they cannot be compiled and evaluated by LINQ.
  /// </remarks>
  public class EvaluatableTreeFindingExpressionVisitor : RelinqExpressionVisitor, IPartialEvaluationExceptionExpressionVisitor
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
      // Parameters are not evaluatable.
      _isCurrentSubtreeEvaluatable = false;
      return base.VisitParameter (expression);
    }

    protected override Expression VisitMethodCall (MethodCallExpression expression)
    {
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

    private bool IsQueryableExpression (Expression expression)
    {
      return expression != null && typeof (IQueryable).GetTypeInfo().IsAssignableFrom (expression.Type.GetTypeInfo());
    }

    public Expression VisitPartialEvaluationExceptionExpression (PartialEvaluationExceptionExpression partialEvaluationExceptionExpression)
    {
      // PartialEvaluationExceptionExpression is not evaluable, and its children aren't either (so we don't visit them).
      _isCurrentSubtreeEvaluatable = false;
      return partialEvaluationExceptionExpression;
    }
  }
}
