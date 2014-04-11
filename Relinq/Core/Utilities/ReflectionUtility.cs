// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Linq.Utilities
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
        var message = string.Format ("Expected a type implementing IEnumerable<T>, but found '{0}'.", enumerableType);
        throw new ArgumentException (message, argumentName);
      }

      return itemType;
    }

    public static Type GetMemberReturnType (MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("member", member);

      switch (member.MemberType)
      {
        case MemberTypes.Property:
          return ((PropertyInfo) member).PropertyType;
        case MemberTypes.Field:
          return ((FieldInfo) member).FieldType;
        case MemberTypes.Method:
          return ((MethodInfo) member).ReturnType;
        default:
          throw new ArgumentException ("Argument must be FieldInfo, PropertyInfo, or MethodInfo.", "member");
      }
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
