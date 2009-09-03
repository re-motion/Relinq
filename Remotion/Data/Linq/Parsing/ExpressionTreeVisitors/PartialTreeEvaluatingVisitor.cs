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
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.TreeEvaluation;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors
{
  /// <summary>
  /// Takes an expression tree and first analyzes it for evaluatable subtrees (using <see cref="EvaluatableTreeFindingVisitor"/>), i.e.
  /// subtrees that can be pre-evaluated before actually generating the query. Examples for evaluatable subtrees are operations on constant
  /// values (constant folding), access to closure variables (variables used by the LINQ query that are defined in an outer scope), or method
  /// calls on known objects or their members. In a second step, it replaces all of the evaluatable subtrees (top-down and non-recursive) by 
  /// their evaluated counterparts.
  /// </summary>
  /// <remarks>
  /// This visitor visits each tree node at most twice: once via the <see cref="EvaluatableTreeFindingVisitor"/> for analysis and once
  /// again to replace nodes if possible (unless the parent node has already been replaced).
  /// </remarks>
  public class PartialTreeEvaluatingVisitor : ExpressionTreeVisitor
  {
    /// <summary>
    /// Takes an expression tree and finds and evaluates all its evaluatable subtrees.
    /// </summary>
    public static Expression EvaluateIndependentSubtrees (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);

      var partialEvaluationInfo = EvaluatableTreeFindingVisitor.Analyze (expressionTree);

      var visitor = new PartialTreeEvaluatingVisitor (expressionTree, partialEvaluationInfo);
      return visitor.VisitExpression (expressionTree);
    }

    // _partialEvaluationInfo contains a list of the expressions that are safe to be evaluated.
    private readonly PartialEvaluationInfo _partialEvaluationInfo;

    private PartialTreeEvaluatingVisitor (Expression treeRoot, PartialEvaluationInfo partialEvaluationInfo)
    {
      ArgumentUtility.CheckNotNull ("treeRoot", treeRoot);
      ArgumentUtility.CheckNotNull ("partialEvaluationInfo", partialEvaluationInfo);

      _partialEvaluationInfo = partialEvaluationInfo;
    }

    protected override Expression VisitUnknownExpression (Expression expression)
    {
      //ignore
      return expression;
    }

    protected override Expression VisitExpression (Expression expression)
    {
      // Only evaluate expressions which do not use any of the surrounding parameter expressions. Don't evaluate
      // lambda expressions (even if you could), we want to analyze those later on.
      if (expression == null)
        return null;
      else if (expression.NodeType != ExpressionType.Lambda && _partialEvaluationInfo.IsEvaluatableExpression (expression))
        return EvaluateSubtree (expression);
      else
        return base.VisitExpression (expression);
    }

    /// <summary>
    /// Evaluates an evaluatable <see cref="Expression"/> subtree, i.e. an independent expression tree that is compilable and executable
    /// without any data being passed in. The result of the evaluation is returned as a <see cref="ConstantExpression"/>; if the subtree
    /// is already a <see cref="ConstantExpression"/>, no evaluation is performed.
    /// </summary>
    /// <param name="subtree">The subtree to be evaluated.</param>
    /// <returns>A <see cref="ConstantExpression"/> holding the result of the evaluation.</returns>
    protected ConstantExpression EvaluateSubtree (Expression subtree)
    {
      ArgumentUtility.CheckNotNull ("subtree", subtree);

      if (subtree.NodeType == ExpressionType.Constant)
        return (ConstantExpression) subtree;
      else
      {
        LambdaExpression lambdaWithoutParameters = Expression.Lambda (subtree);
        object value = lambdaWithoutParameters.Compile ().DynamicInvoke ();
        return Expression.Constant (value, subtree.Type);
      }
    }
  }
}