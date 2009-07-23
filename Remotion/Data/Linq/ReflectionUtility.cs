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

namespace Remotion.Data.Linq
{
  public static class ReflectionUtility
  {
    public static MethodInfo GetMethod<T> (Expression<Func<T>> wrappedCall)
    {
      ArgumentUtility.CheckNotNull ("wrappedCall", wrappedCall);
      return ((MethodCallExpression) wrappedCall.Body).Method;
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

    public static Type TryGetItemTypeOfIEnumerable (Type possibleEnumerableType)
    {
      ArgumentUtility.CheckNotNull ("possibleEnumerableType", possibleEnumerableType);

      Type implementedEnumerableInterface = GetImplementedIEnumerableType (possibleEnumerableType);
      if (implementedEnumerableInterface == null)
        return null;
      else
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