using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.WhereParser
{
  public class MethodCallExpressionParser
  {
    private readonly WhereClause _whereClause;
    private readonly Func<Expression, ICriterion> _parsingCall;

    public MethodCallExpressionParser (WhereClause whereClause, Func<Expression, ICriterion> parsingCall)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      ArgumentUtility.CheckNotNull ("parsingCall", parsingCall);

      _whereClause = whereClause;
      _parsingCall = parsingCall;
    }

    public ICriterion Parse (MethodCallExpression expression)
    {
      if (expression.Method.Name == "StartsWith")
        return CreateLike (expression, ((ConstantExpression) expression.Arguments[0]).Value + "%");
      else if (expression.Method.Name == "EndsWith")
        return CreateLike (expression, "%" + ((ConstantExpression) expression.Arguments[0]).Value);
      else if (expression.Method.Name == "Contains")
        return CreateContains ((SubQueryExpression) expression.Arguments[0], expression.Arguments[1]);

      throw ParserUtility.CreateParserException ("StartsWith, EndsWith", expression.Method.Name, "method call expression in where condition",
          _whereClause.BoolExpression);
    }

    private BinaryCondition CreateLike (MethodCallExpression expression, string pattern)
    {
      return new BinaryCondition (_parsingCall (expression.Object), new Constant (pattern), BinaryCondition.ConditionKind.Like);
    }

    private BinaryCondition CreateContains (SubQueryExpression subQueryExpression, Expression itemExpression)
    {
      return new BinaryCondition (new SubQuery (subQueryExpression.QueryModel, null), _parsingCall (itemExpression),
          BinaryCondition.ConditionKind.Contains);
    }
  }
}