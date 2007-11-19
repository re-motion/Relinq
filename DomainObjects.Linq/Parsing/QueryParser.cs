using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Parsing
{
  public class QueryParser
  {
    private readonly Expression _expression;

    public QueryParser (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _expression = expression;
    }

    public QueryExpression GetParsedQuery ()
    {
      MethodCallExpression selectExpression = GetTypedExpression<MethodCallExpression> (_expression, "Select");
      ConstantExpression querySourceExpression = GetTypedExpression<ConstantExpression> (selectExpression.Arguments[0], "query source");
      IQueryable querySource = GetTypedExpression<IQueryable> (querySourceExpression.Value, "query source");
      UnaryExpression selectUnaryExpression = GetTypedExpression<UnaryExpression> (selectExpression.Arguments[1], "select lambda");
      LambdaExpression selectLambdaExpression = GetTypedExpression<LambdaExpression> (selectUnaryExpression.Operand, "select lambda");
      if (selectLambdaExpression.Parameters.Count != 1)
        throw new QueryParserException ("Expected lambda expression with one argument for select clause, found "
            + selectLambdaExpression.Parameters.Count + " arguments.", selectLambdaExpression, _expression);
      
      ParameterExpression fromIdentifier = selectLambdaExpression.Parameters[0];
      Expression selectBody = selectLambdaExpression.Body;

      FromClause fromClause = new FromClause (fromIdentifier, querySource);
      SelectClause selectClause = new SelectClause (selectBody);
      QueryBody queryBody = new QueryBody (selectClause);
      return new QueryExpression (fromClause, queryBody);
    }

    private T GetTypedExpression<T> (object expression, string context)
    {
      if (!(expression is T))
      {
        string message = string.Format ("Expected {0} for {1}, found {2} ({3}).", typeof (T).Name, context, expression.GetType().Name, expression);
        throw new QueryParserException (message, expression, _expression);
      }
      else
        return (T) expression;
    }
  }

  internal class QueryParserException : Exception
  {
    public QueryParserException (string message, object parsedExpression, Expression expressionTree)
        : base (message)
    {
      ParsedExpression = parsedExpression;
      ExpressionTree = expressionTree;
    }

    public object ParsedExpression { get; private set; }
    public object ExpressionTree { get; private set; }
  }
}