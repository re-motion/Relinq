using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Text;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public static class ParserUtility
  {
    public static T GetTypedExpression<T> (object expression, string context, Expression expressionTreeRoot)
    {
      if (!(expression is T))
      {
        throw CreateParserException (typeof (T).Name, expression, context, expressionTreeRoot);
      }
      else
        return (T) expression;
    }

    public static ParserException CreateParserException (object expected, object expression, string context, Expression expressionTreeRoot)
    {
      return CreateParserException (expected, expression, context, expressionTreeRoot, null);
    }

    public static ParserException CreateParserException (object expected, object expression, string context, Expression expressionTreeRoot,
                                                              Exception inner)
    {
      string message = string.Format ("Expected {0} for {1}, found {2} ({3}).", expected, context, expression.GetType ().Name, expression);
      return new ParserException (message, expression, expressionTreeRoot, inner);
    }

    public static string CheckMethodCallExpression (MethodCallExpression methodCallExpression, Expression expressionTreeRoot,
                                                    params string[] expectedMethodNames)
    {
      if (!((IList<string>)expectedMethodNames).Contains (methodCallExpression.Method.Name))
      {
        string message = string.Format ("Expected one of '{0}', but found '{1}' at position {2} in tree {3}.",
            SeparatedStringBuilder.Build (", ", expectedMethodNames),methodCallExpression.Method.Name, methodCallExpression, expressionTreeRoot);
        throw new ParserException (message, methodCallExpression, expressionTreeRoot, null);
      }
      else
        return methodCallExpression.Method.Name;
    }
  }
}