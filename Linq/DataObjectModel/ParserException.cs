using System;
using System.Linq.Expressions;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public class ParserException : Exception
  {
    public ParserException (string message)
        : this (message, null, null, null)
    {
    }

    public ParserException (string message, Exception inner)
        : base (message, inner)
    {
    }

    public ParserException (string message, object parsedExpression, Expression expressionTree, Exception inner)
        : base (message, inner)
    {
      ParsedExpression = parsedExpression;
      ExpressionTree = expressionTree;
    }

    public object ParsedExpression { get; private set; }
    public Expression ExpressionTree { get; private set; }
  }
}