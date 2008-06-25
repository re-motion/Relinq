using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class LikeParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;
    private readonly Expression _expressionTreeRoot;

    public LikeParser (Expression expressionTreeRoot, WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _expressionTreeRoot = expressionTreeRoot;
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (MethodCallExpression methodCallExpression, ParseContext parseContext)
    {
      if (methodCallExpression.Method.Name == "StartsWith")
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "StartsWith", 1, _expressionTreeRoot);
        ParserUtility.CheckParameterType<ConstantExpression> (methodCallExpression, "StartsWith", 0, _expressionTreeRoot);
        return CreateLike (methodCallExpression, ((ConstantExpression) methodCallExpression.Arguments[0]).Value + "%", parseContext);
      }
      else if (methodCallExpression.Method.Name == "EndsWith")
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "EndsWith", 1, _expressionTreeRoot);
        ParserUtility.CheckParameterType<ConstantExpression> (methodCallExpression, "EndsWith", 0, _expressionTreeRoot);
        return CreateLike (methodCallExpression, "%" + ((ConstantExpression) methodCallExpression.Arguments[0]).Value, parseContext);
      }
      else if (methodCallExpression.Method.Name == "Contains" && !methodCallExpression.Method.IsGenericMethod)
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "Contains", 1, _expressionTreeRoot);
        ParserUtility.CheckParameterType<ConstantExpression> (methodCallExpression, "Contains", 0, _expressionTreeRoot);
        return CreateLike (methodCallExpression, "%" + ((ConstantExpression) methodCallExpression.Arguments[0]).Value + "%", parseContext);
      }
      throw ParserUtility.CreateParserException ("StartsWith, EndsWith, Contains with no expression", methodCallExpression.Method.Name,
          "method call expression in where condition", _expressionTreeRoot);
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((MethodCallExpression) expression, parseContext);
    }

    public bool CanParse (Expression expression)
    {
      var methodCallExpression = expression as MethodCallExpression;
      if (methodCallExpression != null)
      {
        if (methodCallExpression.Method.Name == "StartsWith" ||
          methodCallExpression.Method.Name == "EndsWith" ||
          (methodCallExpression.Method.Name == "Contains" && !methodCallExpression.Method.IsGenericMethod))
          return true;
      }
      return false;
    }

    private BinaryCondition CreateLike (MethodCallExpression expression, string pattern, ParseContext parseContext)
    {
      return new BinaryCondition (_parserRegistry.GetParser (expression.Object).Parse (expression.Object, parseContext), new Constant (pattern), BinaryCondition.ConditionKind.Like);
    }
  }
}