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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Linq.Utilities
{
  public static class GenericEnumerableReflectionUtility
  {
    public static Type TryGetItemTypeOfClosedGenericIEnumerable (Type possibleEnumerableType)
    {
      ArgumentUtility.CheckNotNull ("possibleEnumerableType", possibleEnumerableType);

      var possibleEnumerableTypeInfo = possibleEnumerableType.GetTypeInfo();

      if (possibleEnumerableTypeInfo.IsArray)
        return possibleEnumerableTypeInfo.GetElementType();

      if (!IsIEnumerable (possibleEnumerableTypeInfo))
        return null;

      if (possibleEnumerableTypeInfo.IsGenericTypeDefinition)
        return null;

      if (possibleEnumerableTypeInfo.IsGenericType && possibleEnumerableType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
        return possibleEnumerableTypeInfo.GenericTypeArguments[0];

      var implementedEnumerableInterface = possibleEnumerableTypeInfo.ImplementedInterfaces
          .Select (t => t.GetTypeInfo())
          .FirstOrDefault (IsGenericIEnumerable);

      if (implementedEnumerableInterface == null)
        return null;

      Assertion.DebugAssert (implementedEnumerableInterface.IsGenericType);
      return implementedEnumerableInterface.GenericTypeArguments[0];
    }

    private static bool IsIEnumerable (TypeInfo type)
    {
      return typeof (IEnumerable).GetTypeInfo().IsAssignableFrom (type);
    }

    private static bool IsGenericIEnumerable (TypeInfo enumerableType)
    {
      return IsIEnumerable (enumerableType)
             && enumerableType.IsGenericType
             && enumerableType.GetGenericTypeDefinition() == typeof (IEnumerable<>);
    }
  }
}