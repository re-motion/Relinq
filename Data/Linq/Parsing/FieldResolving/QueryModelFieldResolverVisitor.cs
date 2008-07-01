using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Visitor;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  /// <summary>
  /// removes transparent identifier from a expression representing a field access
  /// </summary>
  public class QueryModelFieldResolverVisitor : ExpressionTreeVisitor
  {
    public class Result
    {
      public Result (Expression reducedExpression, IResolveableClause fromClause)
      {
        ArgumentUtility.CheckNotNull ("fromClause", fromClause);

        ReducedExpression = reducedExpression;
        ResolveableClause = fromClause;
      }

      public Expression ReducedExpression { get; private set; }
      public IResolveableClause ResolveableClause { get; private set; }
    }

    private readonly QueryModel _queryModel;

    private IResolveableClause _clause;

    public QueryModelFieldResolverVisitor (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      _queryModel = queryModel;
    }

    public Result ParseAndReduce (Expression expression)
    {
      //_fromClause = null;
      _clause = null;
      
      Expression reducedExpression = VisitExpression (expression);
      //if (_fromClause != null)
      if (_clause != null)
        return new Result (reducedExpression, _clause);
      else
        return null;
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _clause = _queryModel.GetResolveableClause (expression.Name, expression.Type);
      if (_clause != null)
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