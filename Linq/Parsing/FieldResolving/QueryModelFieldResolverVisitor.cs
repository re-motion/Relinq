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
  public class QueryModelFieldResolverVisitor : ExpressionTreeVisitor
  {
    public class Result
    {
      public Result (Expression reducedExpression, FromClauseBase fromClause)
      {
        ArgumentUtility.CheckNotNull ("fromClause", fromClause);

        ReducedExpression = reducedExpression;
        FromClause = fromClause;
      }

      public Expression ReducedExpression { get; private set; }
      public FromClauseBase FromClause { get; private set; }
    }

    private readonly QueryModel _queryModel;

    private FromClauseBase _fromClause;

    public QueryModelFieldResolverVisitor (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      _queryModel = queryModel;
    }

    public Result ParseAndReduce (Expression expression)
    {
      _fromClause = null;
      
      Expression reducedExpression = VisitExpression (expression);
      if (_fromClause != null)
        return new Result (reducedExpression, _fromClause);
      else
        return null;
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _fromClause = _queryModel.GetFromClause (expression.Name, expression.Type);
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