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
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Remotion.Utilities
{
  static partial class NullableTypeUtility
  {
    /// <summary>
    /// Determines whether a type is nullable, ie. whether variables of it can be assigned <see langword="null"/>.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>
    /// true if <paramref name="type"/> is nullable; otherwise, false.
    /// </returns>
    /// <remarks>
    /// A type is nullable if it is a reference type or a nullable value type. This method returns false only for non-nullable value types.
    /// </remarks>
    public static bool IsNullableType (Type type)
    {
      if (type == null)
        throw new ArgumentNullException ("type");

      return IsNullableType_NoArgumentCheck (type);
    }

    internal static bool IsNullableType_NoArgumentCheck (Type expectedType)
    {
      return !expectedType.GetTypeInfo().IsValueType || Nullable.GetUnderlyingType (expectedType) != null;
    }

    public static Type GetNullableType (Type type)
    {
      if (type == null)
        throw new ArgumentNullException ("type");

      if (IsNullableType (type))
        return type;
      else
        return typeof (Nullable<>).MakeGenericType (type);
    }

    public static Type GetBasicType (Type type)
    {
      if (type == null)
        throw new ArgumentNullException ("type");

      return Nullable.GetUnderlyingType (type) ?? type;
    }
  }
}
