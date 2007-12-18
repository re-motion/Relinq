using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing.TreeEvaluation;
using Rubicon.Data.Linq.Visitor;

namespace Rubicon.Data.Linq.Parsing.TreeEvaluation
{
  public sealed class PartialTreeEvaluator : ExpressionTreeVisitor
  {
    // _parentEvaluatableSubtrees is a list of subtrees that have been found suitable for evaluation when the tree root's parent lambda expression
    // was analyzed. When a node is encountered and it is a member of this list, it will be evaluated. When a LambdaExpression is encountered,
    // a new set of evaluatable subtrees will be calculated for the lambda's body.
    private readonly HashSet<Expression> _parentEvaluatableSubtrees;
    private readonly Expression _evaluatedTree;

    public PartialTreeEvaluator (Expression treeRoot)
    {
      // The tree root has no parent lambda, so create one to get an initial list of filtered subtrees.
      _parentEvaluatableSubtrees = GetFilteredSubtrees (Expression.Lambda (treeRoot));
      _evaluatedTree = VisitExpression (treeRoot);
    }

    private PartialTreeEvaluator (Expression treeRoot, HashSet<Expression> parentEvaluatableSubtrees)
    {
      _parentEvaluatableSubtrees = parentEvaluatableSubtrees;
      _evaluatedTree = VisitExpression (treeRoot);
    }

    public Expression GetEvaluatedTree ()
    {
      return _evaluatedTree;
    }

    // When any node is processed
    protected override Expression VisitExpression (Expression expression)
    {
      if (expression == null)
        return null;
      else if (expression.NodeType != ExpressionType.Lambda && _parentEvaluatableSubtrees.Contains (expression))
        return EvaluateSubtree (expression);
      else
        return base.VisitExpression (expression);
    }

    // When a lambda expression is encountered, its body subtree is first filtered for subtrees that do not include the lambda's parameters
    // (exluding any subtrees that aren't included in _parentEvaluatableSubtrees). Then, these subtrees are recursively analyzed with a separate
    // PartialTreeEvaluator instance.
    protected override Expression VisitLambdaExpression (LambdaExpression expression)
    {
      HashSet<Expression> evaluatableSubtrees = GetFilteredSubtrees (expression);
      PartialTreeEvaluator childEvaluator = new PartialTreeEvaluator (expression.Body, evaluatableSubtrees);
      Expression newBody = childEvaluator.GetEvaluatedTree ();
      if (newBody != expression.Body)
        return Expression.Lambda (expression.Type, newBody, expression.Parameters);
      else
        return expression;
    }

    private HashSet<Expression> GetFilteredSubtrees (LambdaExpression lambdaExpression)
    {
      FilteredSubtreeFinder finder = new FilteredSubtreeFinder (lambdaExpression.Body,
          delegate (Expression currentNode)
          {
            ParameterExpression currentNodeAsParameterExpression = currentNode as ParameterExpression;
            return (_parentEvaluatableSubtrees == null || _parentEvaluatableSubtrees.Contains (currentNode))
                && (currentNodeAsParameterExpression == null || !lambdaExpression.Parameters.Contains (currentNodeAsParameterExpression));
          });
      return finder.GetFilteredSubtrees ();
    }

    private Expression EvaluateSubtree (Expression subtree)
    {
      if (subtree.NodeType == ExpressionType.Constant)
        return subtree;
      else
      {
        LambdaExpression lambdaWithoutParameters = Expression.Lambda (subtree);
        object value = lambdaWithoutParameters.Compile().DynamicInvoke();
        return Expression.Constant (value, subtree.Type);
      }
    }
  }
}