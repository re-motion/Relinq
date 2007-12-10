using System;
using System.Linq.Expressions;

namespace Rubicon.Data.Linq.Parsing
{
  public class QueryParserException : Exception
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