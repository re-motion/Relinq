using System.Collections.Generic;
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
      public Result (MemberInfo[] members, ParameterExpression parameter)
        : this ()
      {
        ArgumentUtility.CheckNotNull ("parameter", parameter);

        Members = members;
        Parameter = parameter;
      }

      public MemberInfo[] Members { get; private set; }
      public ParameterExpression Parameter { get; private set; }
    }

    private ParameterExpression _parameterExpression;
    private List<MemberInfo> _members;
    private Expression _expressionTreeRoot;

    public Result ParseFieldAccess (Expression fieldAccessExpression, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      _parameterExpression = null;
      _members = new List<MemberInfo>();
      _expressionTreeRoot = expressionTreeRoot;

      VisitExpression (fieldAccessExpression);
      return new Result (_members.ToArray(), _parameterExpression);
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
      _members.Add(expression.Member);
      return base.VisitMemberExpression (expression);
    }
  }
}