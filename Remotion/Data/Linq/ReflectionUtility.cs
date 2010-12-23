// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq
{
  public static class ReflectionUtility
  {
    public static MethodInfo GetMethod<T> (Expression<Func<T>> wrappedCall)
    {
      ArgumentUtility.CheckNotNull ("wrappedCall", wrappedCall);

      switch (wrappedCall.Body.NodeType)
      {
        case ExpressionType.Call:
          return ((MethodCallExpression) wrappedCall.Body).Method;
        case ExpressionType.MemberAccess:
          var memberExpression = (MemberExpression) wrappedCall.Body;
          var property = memberExpression.Member as PropertyInfo;
          var method = property != null ? property.GetGetMethod() : null;
          if (method != null)
            return method;
          break;
      }

      throw new ArgumentException (string.Format ("Cannot extract a method from the given expression '{0}'.", wrappedCall.Body), "wrappedCall");
    }

    public static Type GetItemTypeOfIEnumerable (Type enumerableType, string argumentName)
    {
      ArgumentUtility.CheckNotNull ("enumerableType", enumerableType);
      ArgumentUtility.CheckNotNullOrEmpty ("argumentName", argumentName);

      Type itemType = TryGetItemTypeOfIEnumerable (enumerableType);
      if (itemType == null)
      {
        var message = string.Format ("Expected a type implementing IEnumerable<T>, but found '{0}'.", enumerableType.FullName);
        throw new ArgumentTypeException (message, argumentName, typeof (IEnumerable<>), enumerableType);
      }

      return itemType;
    }

    public static Type GetFieldOrPropertyType (MemberInfo fieldOrProperty)
    {
      if (fieldOrProperty is FieldInfo)
        return ((FieldInfo) fieldOrProperty).FieldType;
      else if (fieldOrProperty is PropertyInfo)
        return ((PropertyInfo) fieldOrProperty).PropertyType;
      else
        throw new ArgumentException ("Argument must be FieldInfo or PropertyInfo.", "fieldOrProperty");
    }

    public static Type TryGetItemTypeOfIEnumerable (Type possibleEnumerableType)
    {
      ArgumentUtility.CheckNotNull ("possibleEnumerableType", possibleEnumerableType);
      
      if (possibleEnumerableType.IsArray)
        return possibleEnumerableType.GetElementType ();

      Type implementedEnumerableInterface = GetImplementedIEnumerableType (possibleEnumerableType);
      if (implementedEnumerableInterface == null)
      {
        return null;
      }
      else
      {
        if (implementedEnumerableInterface.IsGenericType)
          return implementedEnumerableInterface.GetGenericArguments ()[0];
        else
          return typeof (object);
      }
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
                where IsIEnumerable (i)
                let genericArgsCount = i.IsGenericType ? i.GetGenericArguments ().Length : 0
                orderby genericArgsCount descending
                select i).FirstOrDefault();
      }
    }

    private static bool IsIEnumerable (Type enumerableType)
    {
      return enumerableType == typeof (IEnumerable) 
          || (enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition() == typeof (IEnumerable<>));
    }
  }
}
