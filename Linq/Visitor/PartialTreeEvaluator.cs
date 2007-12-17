using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Visitor;

namespace Rubicon.Data.Linq.Visitor
{
  public class PartialTreeEvaluator : ExpressionTreeVisitor
  {
    public Expression EvaluateTree (Expression treeRoot)
    {
      return VisitExpression (treeRoot);
    }

    protected override Expression VisitLambdaExpression (LambdaExpression expression)
    {
      if (CanBeEvaluated (expression))
      {
        object lambdaValue = EvaluateLambda (expression.Body);
        return Expression.Constant (lambdaValue, expression.Type);
      }
      else
        return base.VisitLambdaExpression (expression);
    }

    private bool CanBeEvaluated (LambdaExpression lambdaExpression)
    {
      return GetFilteredSubtrees (lambdaExpression).Contains (lambdaExpression.Body);
    }

    private HashSet<Expression> GetFilteredSubtrees (LambdaExpression lambdaExpression)
    {
      FilteredSubtreeFinder finder = new FilteredSubtreeFinder (lambdaExpression.Body,
          delegate (Expression currentNode)
          {
            ParameterExpression currentNodeAsParameterExpression = currentNode as ParameterExpression;
            return currentNodeAsParameterExpression == null || !lambdaExpression.Parameters.Contains (currentNodeAsParameterExpression);
          });
      return finder.GetParameterlessSubtrees ();
    }

    private object EvaluateLambda (Expression expressionBody)
    {
      LambdaExpression lambdaWithoutParameters = Expression.Lambda (expressionBody);
      return lambdaWithoutParameters.Compile().DynamicInvoke();
    }
  }
}