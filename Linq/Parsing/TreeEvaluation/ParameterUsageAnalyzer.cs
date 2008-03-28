using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Visitor;

namespace Rubicon.Data.Linq.Parsing.TreeEvaluation
{
  public class ParameterUsageAnalyzer : ExpressionTreeVisitor
  {
    public ParameterUsageAnalyzer ()
    {
      Usage = new ParameterUsage ();
      CurrentExpressions = new Stack<Expression> ();
    }

    public ParameterUsage Usage { get; private set; }
    protected Stack<Expression> CurrentExpressions { get; private set; }

    public void Analyze (Expression expression)
    {
      VisitExpression (expression);
    }

    protected override Expression VisitExpression (Expression expression)
    {
      if (expression is ConstantExpression)
        return expression;

      PrepareExpression(expression);
      base.VisitExpression (expression);
      FinishExpression();
      
      return expression;
    }

    protected void PrepareExpression (Expression expression)
    {
      if (!Usage.DeclaredParameters.ContainsKey (expression))
        Usage.DeclaredParameters.Add (expression, new HashSet<ParameterExpression> ());
      if (!Usage.UsedParameters.ContainsKey (expression))
        Usage.UsedParameters.Add (expression, new HashSet<ParameterExpression> ());

      CurrentExpressions.Push (expression);
    }

    protected void FinishExpression ()
    {
      CurrentExpressions.Pop ();
    }

    protected override Expression VisitLambdaExpression (LambdaExpression expression)
    {
      foreach (ParameterExpression declaredParameter in expression.Parameters)
      {
        foreach (Expression currentExpression in CurrentExpressions)
          Usage.DeclaredParameters[currentExpression].Add (declaredParameter);
      }

      VisitExpression (expression.Body);
      return expression;
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      foreach (Expression currentExpression in CurrentExpressions)
        Usage.UsedParameters[currentExpression].Add (expression);

      return expression;
    }
  }
}