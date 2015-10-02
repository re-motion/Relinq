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
using JetBrains.Annotations;
using Remotion.Utilities;

namespace Remotion.Linq.Utilities
{
  /// <summary>
  /// Provider a utility API for dealing with the item type of generic collections.
  /// </summary>
  public static class ItemTypeReflectionUtility
  {
    /// <summary>
    /// Tries to extract the item type from the input <see cref="Type"/>.
    /// </summary>
    /// <param name="possibleEnumerableType">
    /// The <see cref="Type"/> that might be an implementation of the <see cref="IEnumerable{T}"/> interface. Must not be <see langword="null" />.
    /// </param>
    /// <param name="itemType">An output parameter containing the extracted item <see cref="Type"/> or <see langword="null" />.</param>
    /// <returns><see langword="true" /> if an <paramref name="itemType"/> could be extracted, otherwise <see langword="false" />.</returns>
    [ContractAnnotation ("=>true, itemType:notnull; =>false, itemType:null")]
    public static bool TryGetItemTypeOfClosedGenericIEnumerable ([NotNull]Type possibleEnumerableType, out Type itemType)
    {
      ArgumentUtility.CheckNotNull ("possibleEnumerableType", possibleEnumerableType);

      var possibleEnumerableTypeInfo = possibleEnumerableType.GetTypeInfo();

      if (possibleEnumerableTypeInfo.IsArray)
      {
        itemType = possibleEnumerableTypeInfo.GetElementType();
        return true;
      }

      if (!IsIEnumerable (possibleEnumerableTypeInfo))
      {
        itemType = null;
        return false;
      }

      if (possibleEnumerableTypeInfo.IsGenericTypeDefinition)
      {
        itemType = null;
        return false;
      }

      if (possibleEnumerableTypeInfo.IsGenericType && possibleEnumerableType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
      {
        itemType = possibleEnumerableTypeInfo.GenericTypeArguments[0];
        return true;
      }

      var implementedEnumerableInterface = possibleEnumerableTypeInfo.ImplementedInterfaces
          .Select (t => t.GetTypeInfo())
          .FirstOrDefault (IsGenericIEnumerable);

      if (implementedEnumerableInterface == null)
      {
        itemType = null;
        return false;
      }

      Assertion.DebugAssert (implementedEnumerableInterface.IsGenericType);
      itemType = implementedEnumerableInterface.GenericTypeArguments[0];
      return true;
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