using System;
using System.Linq.Expressions;

namespace Rubicon.Data.Linq.Parsing
{
  public class QueryParserException : Exception
  {
    public QueryParserException (string message)
        : this (message, null, null, null)
    {
    }

    public QueryParserException (string message, Exception inner)
      : base (message, inner)
    {
    }

    public QueryParserException (string message, object parsedExpression, Expression expressionTree, Exception inner)
      : base (message, inner)
    {
      ParsedExpression = parsedExpression;
      ExpressionTree = expressionTree;
    }

    public object ParsedExpression { get; private set; }
    public Expression ExpressionTree { get; private set; }
  }
}