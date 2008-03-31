using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing.TreeEvaluation;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.TreeEvaluationTest
{
  public class TestableParameterUsageAnalyzer : PartialEvaluationPreAnalyzer
  {
    public bool VisitBinaryExpressionCalled { get; set; }
    public Expression StackTopInVisitBinary { get; set; }

    public new Stack<Expression> CurrentExpressions
    {
      get { return base.CurrentExpressions; }
    }

    public new void PrepareExpression (Expression expression)
    {
      base.PrepareExpression (expression);
    }

    public new void FinishExpression ()
    {
      base.FinishExpression ();
    }

    public new Expression VisitExpression (Expression expression)
    {
      return base.VisitExpression (expression);
    }

    public new Expression VisitLambdaExpression (LambdaExpression expression)
    {
      return base.VisitLambdaExpression (expression);
    }

    public new Expression VisitParameterExpression (ParameterExpression expression)
    {
      return base.VisitParameterExpression (expression);
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      VisitBinaryExpressionCalled = true;
      StackTopInVisitBinary = CurrentExpressions.Peek();
      return base.VisitBinaryExpression (expression);
    }
  }
}