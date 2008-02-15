using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Visitor;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  /// <summary>
  /// removes transparent identifier from a expression representing a field access
  /// </summary>
  public class QueryExpressionFieldResolverVisitor : ExpressionTreeVisitor
  {
    public struct Result
    {
      public Result (Expression reducedExpression, FromClauseBase fromClause) : this ()
      {
        ArgumentUtility.CheckNotNull ("fromClause", fromClause);

        ReducedExpression = reducedExpression;
        FromClause = fromClause;
      }

      public Expression ReducedExpression { get; private set; }
      public FromClauseBase FromClause { get; private set; }
    }

    private readonly QueryExpression _queryExpression;

    private FromClauseBase _fromClause;

    public QueryExpressionFieldResolverVisitor (QueryExpression queryExpression)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);
      _queryExpression = queryExpression;
    }

    public Result ParseAndReduce (Expression expression)
    {
      _fromClause = null;
      Expression reducedExpression = VisitExpression (expression);
      if (_fromClause == null)
      {
        string message = string.Format ("The field access expression '{0}' does not contain a from clause identifier.", expression);
        throw new FieldAccessResolveException (message);
      }
      return new Result (reducedExpression, _fromClause);
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _fromClause = _queryExpression.GetFromClause (expression.Name, expression.Type);
      if (_fromClause != null)
        return base.VisitParameterExpression (expression);
      else
        return null;
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = VisitExpression (expression.Expression);
      if (newExpression == null)
      {
        ParameterExpression newParameterExpression = Expression.Parameter (expression.Type, expression.Member.Name);
        return VisitExpression (newParameterExpression);
      }
      else if (newExpression != expression.Expression)
        return Expression.MakeMemberAccess (newExpression, expression.Member);
      else
        return expression;
    }
  }
}