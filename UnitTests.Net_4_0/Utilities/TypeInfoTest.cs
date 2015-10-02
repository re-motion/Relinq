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
using NUnit.Framework;

namespace Remotion.Linq.UnitTests.Utilities
{
  [TestFixture]
  public class TypeInfoTest
  {
    private interface ITestInterface
    {
    }

    private interface IOtherInterface
    {
    }

    private class TestType : ITestInterface
    {
      public TestType ()
      {
      }

      protected TestType (object p)
      {
      }
    }

    private class DerivedType : TestType, IOtherInterface
    {
    }

    private class GenericType<T1, T2>
    {
    }

    private class DerivedGenericType<T> : GenericType<T, string>
    {
    }

    [Test]
    public void IsAssignableFrom_WithAssignableType_ReturnsTrue ()
    {
      var type = typeof (TestType);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsAssignableFrom (typeof (DerivedType).GetTypeInfo()), Is.True);
      Assert.That (typeInfo.IsAssignableFrom (typeof (DerivedType).GetTypeInfo()), Is.EqualTo (type.IsAssignableFrom (typeof (DerivedType))));
    }

    [Test]
    public void IsAssignableFrom_WithNotAssignableType_ReturnsFalse ()
    {
      var type = typeof (DerivedType);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsAssignableFrom (typeof (TestType).GetTypeInfo()), Is.False);
      Assert.That (typeInfo.IsAssignableFrom (typeof (TestType).GetTypeInfo()), Is.EqualTo (type.IsAssignableFrom (typeof (TestType))));
    }

    [Test]
    public void Assembly_ReturnsAssembly ()
    {
      var type = typeof (TestType);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.Assembly, Is.SameAs (GetType().Assembly));
      Assert.That (typeInfo.Assembly, Is.SameAs (type.Assembly));
    }

    [Test]
    public void IsValueType_WithValueType_ReturnsTrue ()
    {
      var type = typeof (int);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsValueType, Is.True);
      Assert.That (typeInfo.IsValueType, Is.EqualTo (type.IsValueType));
    }

    [Test]
    public void IsValueType_WithReferenceType_ReturnsFalse ()
    {
      var type = typeof (string);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsValueType, Is.False);
      Assert.That (typeInfo.IsValueType, Is.EqualTo (type.IsValueType));
    }

    [Test]
    public void IsArray_WithArrayType_ReturnsTrue ()
    {
      var type = typeof (string[]);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsArray, Is.True);
      Assert.That (typeInfo.IsArray, Is.EqualTo (type.IsArray));
    }

    [Test]
    public void IsArray_WithType_ReturnsFalse ()
    {
      var type = typeof (string);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsArray, Is.False);
      Assert.That (typeInfo.IsArray, Is.EqualTo (type.IsArray));
    }

    [Test]
    public void GetElementType_ReturnsElementType ()
    {
      var type = typeof (string[]);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GetElementType(), Is.SameAs (typeof (string)));
      Assert.That (typeInfo.GetElementType(), Is.SameAs (type.GetElementType()));
    }

    [Test]
    public void DeclaredConstructors_WithBaseType_ReturnsPublicAndNonPublicCtors ()
    {
      var type = typeof (TestType);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.DeclaredConstructors.Count(), Is.EqualTo (2));
      Assert.That (
          typeInfo.DeclaredConstructors,
          Is.EquivalentTo (type.GetConstructors (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));
    }

    [Test]
    public void DeclaredConstructors_WithDerivedType_ReturnsOnlyCtorsDeclaredCurrentType ()
    {
      var type = typeof (DerivedType);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.DeclaredConstructors.Count(), Is.EqualTo (1));
      Assert.That (
          typeInfo.DeclaredConstructors,
          Is.EquivalentTo (type.GetConstructors (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)));
    }

    [Test]
    public void ImplementedInterfaces_WithBaseType_ReturnsAllInterfaces ()
    {
      var type = typeof (TestType);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.ImplementedInterfaces, Is.EquivalentTo (new[] { typeof (ITestInterface) }));
      Assert.That (typeInfo.ImplementedInterfaces, Is.EquivalentTo (type.GetInterfaces()));
    }

    [Test]
    public void ImplementedInterfaces_WithDerivedType_ReturnsAllInterfacesFromTypeAndBaseType ()
    {
      var type = typeof (DerivedType);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.ImplementedInterfaces, Is.EquivalentTo (new[] { typeof (ITestInterface), typeof (IOtherInterface) }));
      Assert.That (typeInfo.ImplementedInterfaces, Is.EquivalentTo (type.GetInterfaces()));
    }

    [Test]
    public void IsGenericType_WithOpenGenericType_ReturnsTrue ()
    {
      var type = typeof (IEnumerable<>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsGenericType, Is.True);
      Assert.That (typeInfo.IsGenericType, Is.EqualTo (type.IsGenericType));
    }

    [Test]
    public void IsGenericType_WithPartiallyClosedGenericType_ReturnsTrue ()
    {
      var type = typeof (DerivedGenericType<>).BaseType;
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsGenericType, Is.True);
      Assert.That (typeInfo.IsGenericType, Is.EqualTo (type.IsGenericType));
    }

    [Test]
    public void IsGenericType_WithClosedGenericType_ReturnsTrue ()
    {
      var type = typeof (IEnumerable<string>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsGenericType, Is.True);
      Assert.That (typeInfo.IsGenericType, Is.EqualTo (type.IsGenericType));
    }

    [Test]
    public void IsGenericType_WithNonGenericType_ReturnsFalse ()
    {
      var type = typeof (IEnumerable);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsGenericType, Is.False);
      Assert.That (typeInfo.IsGenericType, Is.EqualTo (type.IsGenericType));
    }

    [Test]
    public void IsGenericTypeDefinition_WithOpenGenericType_ReturnsTrue ()
    {
      var type = typeof (IEnumerable<>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsGenericTypeDefinition, Is.True);
      Assert.That (typeInfo.IsGenericTypeDefinition, Is.EqualTo (type.IsGenericTypeDefinition));
    }

    [Test]
    public void IsGenericTypeDefinition_WithPartiallyClosedGenericType_ReturnsFalse ()
    {
      var type = typeof (DerivedGenericType<>).BaseType;
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsGenericTypeDefinition, Is.False);
      Assert.That (typeInfo.IsGenericTypeDefinition, Is.EqualTo (type.IsGenericTypeDefinition));
    }

    [Test]
    public void IsGenericTypeDefinition_WithClosedGenericType_ReturnsFalse ()
    {
      var type = typeof (IEnumerable<string>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsGenericTypeDefinition, Is.False);
      Assert.That (typeInfo.IsGenericTypeDefinition, Is.EqualTo (type.IsGenericTypeDefinition));
    }

    [Test]
    public void IsGenericTypeDefinition_WithNonGenericType_ReturnsFalse ()
    {
      var type = typeof (IEnumerable);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.IsGenericTypeDefinition, Is.False);
      Assert.That (typeInfo.IsGenericTypeDefinition, Is.EqualTo (type.IsGenericTypeDefinition));
    }

    [Test]
    public void GetGenericTypeDefinition_WithOpenGenericType_ReturnsOpenGenericType ()
    {
      var type = typeof (IEnumerable<>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GetGenericTypeDefinition(), Is.SameAs (typeof (IEnumerable<>)));
      Assert.That (typeInfo.GetGenericTypeDefinition(), Is.EqualTo (type.GetGenericTypeDefinition()));
    }

    [Test]
    public void GetGenericTypeDefinition_WithPartiallyGenericType_ReturnsOpenGenericType ()
    {
      var type = typeof (DerivedGenericType<>).BaseType;
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GetGenericTypeDefinition(), Is.SameAs (typeof (GenericType<,>)));
      Assert.That (typeInfo.GetGenericTypeDefinition(), Is.EqualTo (type.GetGenericTypeDefinition()));
    }

    [Test]
    public void GetGenericTypeDefinition_WithClosedGenericType_ReturnsOpenGenericType ()
    {
      var type = typeof (IEnumerable<string>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GetGenericTypeDefinition(), Is.SameAs (typeof (IEnumerable<>)));
      Assert.That (typeInfo.GetGenericTypeDefinition(), Is.EqualTo (type.GetGenericTypeDefinition()));
    }

    [Test]
    public void GetGenericTypeDefinition_WithNonGenericType_ThrowsInvalidOperationException ()
    {
      var type = typeof (IEnumerable);
      var typeInfo = type.GetTypeInfo();
      Assert.That (() => typeInfo.GetGenericTypeDefinition(), Throws.InvalidOperationException);
      Assert.That (() => type.GetGenericTypeDefinition(), Throws.InvalidOperationException);
    }

    [Test]
    public void ContainsGenericParameters_WithOpenGenericType_ReturnsTrue ()
    {
      var type = typeof (IEnumerable<>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.ContainsGenericParameters, Is.True);
      Assert.That (typeInfo.ContainsGenericParameters, Is.EqualTo (type.ContainsGenericParameters));
    }

    [Test]
    public void ContainsGenericParameters_WithPartiallyClosedGenericType_ReturnsTrue ()
    {
      var type = typeof (DerivedGenericType<>).BaseType;
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.ContainsGenericParameters, Is.True);
      Assert.That (typeInfo.ContainsGenericParameters, Is.EqualTo (type.ContainsGenericParameters));
    }

    [Test]
    public void ContainsGenericParameters_WithClosedGenericType_ReturnsFalse ()
    {
      var type = typeof (IEnumerable<string>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.ContainsGenericParameters, Is.False);
      Assert.That (typeInfo.ContainsGenericParameters, Is.EqualTo (type.ContainsGenericParameters));
    }

    [Test]
    public void ContainsGenericParameters_WithNonGenericType_ReturnsFalse ()
    {
      var type = typeof (IEnumerable);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.ContainsGenericParameters, Is.False);
      Assert.That (typeInfo.ContainsGenericParameters, Is.EqualTo (type.ContainsGenericParameters));
    }

    [Test]
    public void GenericTypeParameters_WithOpenGenericType_ReturnsTypes ()
    {
      var type = typeof (IEnumerable<>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GenericTypeParameters.Length, Is.EqualTo (1));
      Assert.That (typeInfo.GenericTypeParameters, Is.EqualTo (type.GetGenericArguments()));
    }

    [Test]
    public void GenericTypeParameters_WithClosedGenericType_ReturnsEmpty ()
    {
      var type = typeof (IEnumerable<string>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GenericTypeParameters, Is.Empty);
    }

    [Test]
    public void GenericTypeParameters_WithPartiallyClosedGenericType_ReturnsEmpty ()
    {
      var type = typeof (DerivedGenericType<>).BaseType;
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GenericTypeParameters, Is.Empty);
    }

    [Test]
    public void GenericTypeParameters_WithNonGenericType_ReturnsEmoty ()
    {
      var type = typeof (IEnumerable);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GenericTypeParameters, Is.Empty);
    }

    [Test]
    public void GenericArguments_WithOpenGenericType_ReturnsEmpty ()
    {
      var type = typeof (IEnumerable<>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GenericTypeArguments, Is.Empty);
    }

    [Test]
    public void GenericArguments_WithClosedGenericType_ReturnsTypes ()
    {
      var type = typeof (IEnumerable<string>);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GenericTypeArguments, Is.EqualTo (new[] { typeof (string) }));
      Assert.That (typeInfo.GenericTypeArguments, Is.EqualTo (type.GetGenericArguments()));
    }

    [Test]
    public void GenericTypeArguments_WithPartiallyClosedGenericType_ReturnsTypes ()
    {
      var type = typeof (DerivedGenericType<>).BaseType;
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GenericTypeArguments.Length, Is.EqualTo (2));
      Assert.That (typeInfo.GenericTypeArguments, Has.Member (typeof (string)));
      Assert.That (typeInfo.GenericTypeArguments, Is.EqualTo (type.GetGenericArguments()));
    }

    [Test]
    public void GenericArguments_WithNonGenericType_ReturnsEmpty ()
    {
      var type = typeof (IEnumerable);
      var typeInfo = type.GetTypeInfo();
      Assert.That (typeInfo.GenericTypeArguments, Is.Empty);
      Assert.That (typeInfo.GenericTypeArguments, Is.EqualTo (type.GetGenericArguments()));
    }
  }
}