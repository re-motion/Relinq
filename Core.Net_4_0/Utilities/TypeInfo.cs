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
  internal class TypeInfo
  {
    private readonly Type _type;

    public TypeInfo ([NotNull]Type type)
    {
      _type = type;
    }

    public bool IsAssignableFrom (TypeInfo type)
    {
      return _type.IsAssignableFrom (type._type);
    }

    public Assembly Assembly
    {
      get { return _type.Assembly; }
    }

    public bool IsValueType
    {
      get { return _type.IsValueType; }
    }

    public bool IsArray
    {
      get { return _type.IsArray; }
    }

    public Type GetElementType ()
    {
      return _type.GetElementType();
    }

    public ConstructorInfo[] DeclaredConstructors
    {
      get { return _type.GetConstructors (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); }
    }

    public Type[] ImplementedInterfaces
    {
      get { return _type.GetInterfaces(); }
    }

    public bool IsGenericType
    {
      get { return _type.IsGenericType; }
    }

    public bool IsGenericTypeDefinition
    {
      get { return _type.IsGenericTypeDefinition; }
    }

    public Type GetGenericTypeDefinition ()
    {
      return _type.GetGenericTypeDefinition();
    }

    public bool ContainsGenericParameters
    {
      get { return _type.ContainsGenericParameters; }
    }

    public Type[] GenericTypeParameters
    {
      get
      {
        if (!_type.IsGenericTypeDefinition)
          return new Type[0];

        return _type.GetGenericArguments();
      }
    }

    public Type[] GenericTypeArguments
    {
      get
      {
        if (_type.IsGenericTypeDefinition)
          return new Type[0];

        return _type.GetGenericArguments();
      }
    }
  }
}