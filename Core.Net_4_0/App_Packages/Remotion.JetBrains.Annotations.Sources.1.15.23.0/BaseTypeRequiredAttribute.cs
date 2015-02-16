// NOTE: This file was originally generated via JetBrains ReSharper
// and is part of the JetBrains.Annoations for ReSharper. 
// It has since been modified for use in the re-motion framework development.
//
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
// Orignal license header by JetBrains:
// 
// Copyright 2007-2012 JetBrains s.r.o.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

using System;

#pragma warning disable 1591
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace

namespace JetBrains.Annotations
{
  /// <summary>
  /// When applied to target attribute, specifies a requirement for any type which is marked with 
  /// target attribute to implement or inherit specific type or types
  /// </summary>
  /// <example>
  /// <code>
  /// [BaseTypeRequired(typeof(IComponent)] // Specify requirement
  /// public class ComponentAttribute : Attribute 
  /// {}
  /// 
  /// [Component] // ComponentAttribute requires implementing IComponent interface
  /// public class MyComponent : IComponent
  /// {}
  /// </code>
  /// </example>
  [AttributeUsage (AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  [BaseTypeRequired (typeof (Attribute))]
  sealed partial class BaseTypeRequiredAttribute : Attribute
  {
    /// <summary>
    /// Initializes new instance of BaseTypeRequiredAttribute
    /// </summary>
    /// <param name="baseType">Specifies which types are required</param>
    public BaseTypeRequiredAttribute (Type baseType)
    {
      BaseTypes = new[] { baseType };
    }

    /// <summary>
    /// Gets enumerations of specified base types
    /// </summary>
    public Type[] BaseTypes { get; private set; }
  }
}