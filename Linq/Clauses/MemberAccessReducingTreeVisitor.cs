using System.Linq.Expressions;
using Rubicon.Data.Linq.Visitor;

namespace Rubicon.Data.Linq.Clauses
{
  public class MemberAccessReducingTreeVisitor : ExpressionTreeVisitor
  {
    public Expression ReduceInnermostMemberExpression (MemberExpression expression)
    {
      return VisitMemberExpression (expression);
    }

    protected override System.Linq.Expressions.Expression VisitMemberExpression (System.Linq.Expressions.MemberExpression expression)
    {
      if (expression.Expression is MemberExpression)
        return base.VisitMemberExpression (expression);
      else
        return Expression.Parameter (expression.Type, expression.Member.Name);
    }
  }
}