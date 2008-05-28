using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Text;

namespace Remotion.Data.Linq.Parsing
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
      string message;
      if (expression is Expression)
        message = string.Format ("Expected {0} for {1}, found {2} ({3}).", expected, context, expression, expression.GetType ().Name);
      else
        message = string.Format ("Expected {0} for {1}, found {2}.", expected, context, expression);
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

    public static void CheckNumberOfArguments (MethodCallExpression expression, string methodName, int expectedArgumentCount, Expression expressionTreeRoot)
    {
      if (expression.Arguments.Count != expectedArgumentCount)
        throw CreateParserException ("at least " + expectedArgumentCount + " argument", expression.Arguments.Count + " arguments",
            methodName + " method call", expressionTreeRoot);
    }

    public static void CheckParameterType<T> (MethodCallExpression expression, string methodName, int parameterIndex, Expression expressionTreeRoot)
    {
      if (!(expression.Arguments[parameterIndex] is T))
        throw CreateParserException (typeof (T).Name, 
            expression.Arguments[parameterIndex].GetType().Name + " (" + expression.Arguments[parameterIndex] + ")",
            "argument " + parameterIndex + " of " + methodName + " method call", expressionTreeRoot);
    }
  }
}