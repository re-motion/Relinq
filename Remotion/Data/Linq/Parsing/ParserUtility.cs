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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing
{
  public static class ParserUtility
  {
    public static T GetTypedExpression<T> (object expression, string context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      if (!(expression is T))
        throw CreateParserException (typeof (T).Name, expression, context);
      else
        return (T) expression;
    }

    public static ParserException CreateParserException (object expected, object expression, string context)
    {
      ArgumentUtility.CheckNotNull ("expected", expected);
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      string message;
      if (expression is Expression)
        message = string.Format ("Expected {0} for {1}, found '{2}' ({3}).", expected, context, expression, expression.GetType().Name);
      else
        message = string.Format ("Expected {0} for {1}, found '{2}'.", expected, context, expression);
      return new ParserException (message, expression, null);
    }

    public static MethodInfo GetMethod<T> (Expression<Func<T>> wrappedCall)
    {
      ArgumentUtility.CheckNotNull ("wrappedCall", wrappedCall);
      return ((MethodCallExpression) wrappedCall.Body).Method;
    }

    public static Type GetItemTypeOfIEnumerable (Type enumerableType)
    {
      Type implementedEnumerableInterface = GetImplementedIEnumerableType(enumerableType);
      if (implementedEnumerableInterface == null)
      {
        var message = string.Format ("Expected a type implementing IEnumerable<T>, but found '{0}'.", enumerableType.FullName);
        throw new ArgumentTypeException (message, "enumerableType", typeof (IEnumerable<>), enumerableType);
      }

      return implementedEnumerableInterface.GetGenericArguments ()[0];
    }

    private static Type GetImplementedIEnumerableType (Type enumerableType)
    {
      if (IsIEnumerable (enumerableType))
      {
        return enumerableType;
      }
      else
      {
        return (from i in enumerableType.GetInterfaces()
                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IEnumerable<>)
                select i).FirstOrDefault();
      }
    }

    private static bool IsIEnumerable (Type enumerableType)
    {
      return enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition() == typeof (IEnumerable<>);
    }
  }
}