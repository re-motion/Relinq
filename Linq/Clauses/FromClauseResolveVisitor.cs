using System.Linq.Expressions;
using Rubicon.Data.Linq.Visitor;
using System.Reflection;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class FromClauseResolveVisitor : ExpressionTreeVisitor
  {
    public struct Result
    {
      public Result (MemberInfo member, ParameterExpression parameter) : this ()
      {
        ArgumentUtility.CheckNotNull ("parameter", parameter);

        Member = member;
        Parameter = parameter;
      }

      public MemberInfo Member { get; private set; }
      public ParameterExpression Parameter { get; private set; }
    }

    private ParameterExpression _parameterExpression;
    private MemberInfo _member;
    private Expression _expressionTreeRoot;

    public Result ParseFieldAccess (Expression fieldAccessExpression, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      _parameterExpression = null;
      _member = null;
      _expressionTreeRoot = expressionTreeRoot;

      VisitExpression (fieldAccessExpression);
      return new Result (_member, _parameterExpression);
    }

    protected override Expression VisitExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      switch (expression.NodeType)
      {
        case ExpressionType.Parameter:
        case ExpressionType.MemberAccess:
          return base.VisitExpression (expression);
        default:
          string message = string.Format ("Only MemberExpressions and ParameterExpressions can be resolved, found '{0}' in expression '{1}'.",
              expression, _expressionTreeRoot);
          throw new FieldAccessResolveException (message);
      }
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      _parameterExpression = expression;
      return base.VisitParameterExpression (expression);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      _member = expression.Member;
      return base.VisitMemberExpression (expression);
    }
  }
}