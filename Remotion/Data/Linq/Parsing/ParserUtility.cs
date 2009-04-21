// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing
{
  public static class ParserUtility
  {
    public static T GetTypedExpression<T> (object expression, string context, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      if (!(expression is T))
      {
        throw CreateParserException (typeof (T).Name, expression, context, expressionTreeRoot);
      }
      else
        return (T) expression;
    }

    public static ParserException CreateParserException (object expected, object expression, string context, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expected", expected);
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);
      return CreateParserException (expected, expression, context, expressionTreeRoot, null);
    }

    public static ParserException CreateParserException (object expected, object expression, string context, Expression expressionTreeRoot,
                                                         Exception inner)
    {
      ArgumentUtility.CheckNotNull ("expected", expected);
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);
      string message;
      if (expression is Expression)
        message = string.Format ("Expected {0} for {1}, found '{2}' ({3}).", expected, context, expression, expression.GetType ().Name);
      else
        message = string.Format ("Expected {0} for {1}, found '{2}'.", expected, context, expression);
      return new ParserException (message, expression, expressionTreeRoot, inner);
    }

    public static string CheckMethodCallExpression (MethodCallExpression methodCallExpression, Expression expressionTreeRoot,
                                                    params string[] expectedMethodNames)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("expectedMethodNames", expectedMethodNames);

      if (!((IList<string>)expectedMethodNames).Contains (methodCallExpression.Method.Name))
      {
        string message = string.Format ("Expected one of '{0}', but found '{1}' at {2} in tree {3}.",
            SeparatedStringBuilder.Build (", ", expectedMethodNames),methodCallExpression.Method.Name, methodCallExpression, expressionTreeRoot);
        throw new ParserException (message, methodCallExpression, expressionTreeRoot, null);
      }
      else
        return methodCallExpression.Method.Name;
    }

    public static void CheckNumberOfArguments (MethodCallExpression expression, string methodName, int expectedArgumentCount, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("methodName", methodName);
      ArgumentUtility.CheckNotNull ("expectedArgumentCount", expectedArgumentCount);
      if (expression.Arguments.Count != expectedArgumentCount)
        throw CreateParserException ("at least " + expectedArgumentCount + " argument", expression.Arguments.Count + " arguments",
            methodName + " method call", expressionTreeRoot);
    }

    public static void CheckParameterType<T> (MethodCallExpression expression, string methodName, int parameterIndex, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("methodName", methodName);
      ArgumentUtility.CheckNotNull ("parameterIndex", parameterIndex);

      if (!(expression.Arguments[parameterIndex] is T))
        throw CreateParserException (typeof (T).Name, 
            expression.Arguments[parameterIndex].GetType().Name + " (" + expression.Arguments[parameterIndex] + ")",
            "argument " + parameterIndex + " of " + methodName + " method call", expressionTreeRoot);
    }

    public static MethodInfo GetMethod<T> (Expression<Func<T>> wrappedCall)
    {
      ArgumentUtility.CheckNotNull ("wrappedCall", wrappedCall);
      return ((MethodCallExpression) wrappedCall.Body).Method;
    }
  }
}
