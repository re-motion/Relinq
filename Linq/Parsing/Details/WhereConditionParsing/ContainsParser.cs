using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class ContainsParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;
    private readonly Expression _expressionTreeRoot;

    public ContainsParser (Expression expressionTreeRoot, WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _expressionTreeRoot = expressionTreeRoot;
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (MethodCallExpression methodCallExpression, ParseContext parseContext)
    {
      if (methodCallExpression.Method.Name == "Contains" && methodCallExpression.Method.IsGenericMethod)
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "Contains", 2, _expressionTreeRoot);
        ParserUtility.CheckParameterType<SubQueryExpression> (methodCallExpression, "Contains", 0, _expressionTreeRoot);
        return CreateContains ((SubQueryExpression) methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], parseContext);
      }
      throw ParserUtility.CreateParserException ("Contains with expression", methodCallExpression.Method.Name,
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
        if (methodCallExpression.Method.Name == "Contains" && methodCallExpression.Method.IsGenericMethod)
          return true;
      }
      return false;
    }

    private BinaryCondition CreateContains (SubQueryExpression subQueryExpression, Expression itemExpression, ParseContext parseContext)
    {
      SubQuery subQuery = new SubQuery (subQueryExpression.QueryModel, null);
      return new BinaryCondition (subQuery, _parserRegistry.GetParser (itemExpression).Parse (itemExpression, parseContext),
          BinaryCondition.ConditionKind.Contains);
    }
  }
}