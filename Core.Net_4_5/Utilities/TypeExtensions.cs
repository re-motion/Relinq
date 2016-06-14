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
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
  internal static class TypeExtensions
  {
    [CanBeNull]
    public static FieldInfo GetRuntimeField ([NotNull] this Type type, [NotNull] string name)
    {
      return type.GetField (name);
    }

    [CanBeNull]
    public static PropertyInfo GetRuntimeProperty ([NotNull] this Type type, [NotNull] string name)
    {
      return type.GetProperty (name);
    }

    [NotNull]
    public static MethodInfo[] GetRuntimeMethods ([NotNull] this Type type)
    {
      return type.GetMethods();
    }

    [CanBeNull]
    public static MethodInfo GetRuntimeMethod ([NotNull] this Type type, [NotNull] string name, [NotNull] Type[] types)
    {
      return type.GetMethod (name, types);
    }

    [NotNull]
    public static TypeInfo GetTypeInfo ([NotNull] this Type type)
    {
      return new TypeInfo (type);
    }
  }
}