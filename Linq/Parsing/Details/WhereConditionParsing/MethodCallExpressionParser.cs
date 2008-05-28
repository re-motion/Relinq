using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
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
      {
        ParserUtility.CheckNumberOfArguments (expression, "StartsWith", 1, _whereClause.BoolExpression);
        ParserUtility.CheckParameterType<ConstantExpression> (expression, "StartsWith", 0, _whereClause.BoolExpression);
        return CreateLike (expression, ((ConstantExpression) expression.Arguments[0]).Value + "%");
      }
      else if (expression.Method.Name == "EndsWith")
      {
        ParserUtility.CheckNumberOfArguments (expression, "EndsWith", 1, _whereClause.BoolExpression);
        ParserUtility.CheckParameterType<ConstantExpression> (expression, "EndsWith", 0, _whereClause.BoolExpression);
        return CreateLike (expression, "%" + ((ConstantExpression) expression.Arguments[0]).Value);
      }
      else if (expression.Method.Name == "Contains" && expression.Method.IsGenericMethod)
      {
        ParserUtility.CheckNumberOfArguments (expression, "Contains", 2, _whereClause.BoolExpression);
        ParserUtility.CheckParameterType<SubQueryExpression> (expression, "Contains", 0, _whereClause.BoolExpression);
        return CreateContains ((SubQueryExpression) expression.Arguments[0], expression.Arguments[1]);
      }
      else if (expression.Method.Name == "Contains" && !expression.Method.IsGenericMethod)
      {
        ParserUtility.CheckNumberOfArguments (expression, "Contains", 1, _whereClause.BoolExpression);
        ParserUtility.CheckParameterType<ConstantExpression> (expression, "Contains", 0, _whereClause.BoolExpression);
        return CreateLike (expression, "%" + ((ConstantExpression) expression.Arguments[0]).Value + "%");
      }
      else if (expression.Method.Name == "ContainsFulltext")
      {
        return CreateContainsFulltext (expression, (string) ((ConstantExpression) expression.Arguments[1]).Value);
      }

      throw ParserUtility.CreateParserException ("StartsWith, EndsWith, Contains, ContainsFulltext", expression.Method.Name,
          "method call expression in where condition", _whereClause.BoolExpression);
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

    private BinaryCondition CreateContainsFulltext (MethodCallExpression expression, string pattern)
    {
      return new BinaryCondition (_parsingCall (expression.Arguments[0]), new Constant (pattern), BinaryCondition.ConditionKind.ContainsFulltext);
    }
  }
}