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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Utilities;

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
  public class EvaluatableTreeFindingExpressionTreeVisitor : ExpressionTreeVisitor, IPartialEvaluationExceptionExpressionVisitor
  {
    public static PartialEvaluationInfo Analyze (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);

      var visitor = new EvaluatableTreeFindingExpressionTreeVisitor();
      visitor.VisitExpression (expressionTree);
      return visitor._partialEvaluationInfo;
    }

    private readonly PartialEvaluationInfo _partialEvaluationInfo = new PartialEvaluationInfo();
    private bool _isCurrentSubtreeEvaluatable;

    public override Expression VisitExpression (Expression expression)
    {
      if (expression == null)
        return base.VisitExpression (expression);

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
      var visitedExpression = base.VisitExpression (expression);

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

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      // Parameters are not evaluatable.
      _isCurrentSubtreeEvaluatable = false;
      return base.VisitParameterExpression (expression);
    }

    protected override Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      // Method calls are only evaluatable if they do not involve IQueryable objects.

      if (IsQueryableExpression (expression.Object))
        _isCurrentSubtreeEvaluatable = false;

      for (int i = 0; i < expression.Arguments.Count && _isCurrentSubtreeEvaluatable; i++)
      {
        if (IsQueryableExpression (expression.Arguments[i]))
          _isCurrentSubtreeEvaluatable = false;
      }

      return base.VisitMethodCallExpression (expression);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      // MemberExpressions are only evaluatable if they do not involve IQueryable objects.

      if (IsQueryableExpression (expression.Expression))
        _isCurrentSubtreeEvaluatable = false;

      return base.VisitMemberExpression (expression);
    }

    protected override Expression VisitMemberInitExpression (MemberInitExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      VisitMemberBindingList (expression.Bindings);

      // Visit the NewExpression only if the List initializers is evaluatable. It makes no sense to evaluate the ListExpression if the initializers
      // cannot be evaluated.

      if (!_isCurrentSubtreeEvaluatable)
        return expression;

      VisitExpression (expression.NewExpression);
      return expression;
    }

    protected override Expression VisitListInitExpression (ListInitExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      VisitElementInitList (expression.Initializers);

      // Visit the NewExpression only if the List initializers is evaluatable. It makes no sense to evaluate the NewExpression if the initializers
      // cannot be evaluated.

      if (!_isCurrentSubtreeEvaluatable)
        return expression;

      VisitExpression (expression.NewExpression);
      return expression;
    }

    protected internal override Expression VisitUnknownNonExtensionExpression (Expression expression)
    {
      // Ignore
      return expression;
    }

    private bool IsQueryableExpression (Expression expression)
    {
      return expression != null && typeof (IQueryable).IsAssignableFrom (expression.Type);
    }

    public Expression VisitPartialEvaluationExceptionExpression (PartialEvaluationExceptionExpression partialEvaluationExceptionExpression)
    {
      // PartialEvaluationExceptionExpression is not evaluable, and its children aren't either (so we don't visit them).
      _isCurrentSubtreeEvaluatable = false;
      return partialEvaluationExceptionExpression;
    }
  }
}
