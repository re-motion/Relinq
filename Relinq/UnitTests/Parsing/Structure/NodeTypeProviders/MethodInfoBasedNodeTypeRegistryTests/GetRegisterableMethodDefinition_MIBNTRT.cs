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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders.MethodInfoBasedNodeTypeRegistryTests.TestDomain;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders.MethodInfoBasedNodeTypeRegistryTests
{
  [TestFixture]
  public class GetRegisterableMethodDefinition_MethodInfoBasedNodeTypeRegistryTest
  {
    [Test]
    public void Test_OrdinaryMethod ()
    {
      var method = typeof (object).GetMethod ("Equals", BindingFlags.Public | BindingFlags.Instance);
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method, true);

      Assert.That (registerableMethod, Is.SameAs (method));
    }

    [Test]
    public void Test_GenericMethodDefinition ()
    {
      var method = ReflectionUtility.GetMethod (() => Queryable.Count<object> (null)).GetGenericMethodDefinition();
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method, true);

      Assert.That (registerableMethod, Is.SameAs (method));
    }

    [Test]
    public void Test_ClosedGenericMethod ()
    {
      var method = ReflectionUtility.GetMethod (() => Queryable.Count<object> (null));
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method, true);

      Assert.That (registerableMethod, Is.SameAs (method.GetGenericMethodDefinition()));
    }

    [Test]
    public void Test_NonGenericMethod_InGenericTypeDefinition ()
    {
      var method = typeof (GenericClass<>).GetMethod ("NonGenericMethod");
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method, true);

      Assert.That (registerableMethod, Is.SameAs (method));
    }

    [Test]
    public void Test_NonGenericMethod_InClosedGenericType ()
    {
      var method = typeof (GenericClass<int>).GetMethod ("NonGenericMethod");
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method, true);

      Assert.That (registerableMethod, Is.SameAs (typeof (GenericClass<>).GetMethod ("NonGenericMethod")));
    }

    [Test]
    public void Test_ClosedGenericMethod_InClosedGenericType ()
    {
      var method = typeof (GenericClass<int>).GetMethod ("GenericMethod").MakeGenericMethod (typeof (string));
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method, true);

      Assert.That (registerableMethod, Is.SameAs (typeof (GenericClass<>).GetMethod ("GenericMethod")));
    }

    [Test]
    public void Test_NonGenericMethod_InClosedGenericType_HavingOverloadsDistinguishedByParameterCount ()
    {
      var methodName = "NonGenericMethodOverloadedWithGenericParameterFromTypeAndDifferentParameterCount";
      var method1 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters().Length == 1);

      var method2 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters().Length == 2);

      var expectedMethod1 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters().Length == 1);

      var expectedMethod2 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters().Length == 2);

      var registerableMethod1 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method1, true);
      var registerableMethod2 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method2, true);

      Assert.That (registerableMethod1, Is.SameAs (expectedMethod1));
      Assert.That (registerableMethod2, Is.SameAs (expectedMethod2));
    }

    [Test]
    public void Test_NonGenericMethod_InClosedGenericType_HavingOverloadsDistinguishedByParameterName ()
    {
      var methodName = "NonGenericMethodOverloadedWithGenericParameterFromTypeAndDifferentParameterName";
      var method1 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "Int32");

      var method2 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "String");

      var expectedMethod1 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "T1");

      var expectedMethod2 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "T2");

      var registerableMethod1 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method1, true);
      var registerableMethod2 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method2, true);

      Assert.That (registerableMethod1, Is.SameAs (expectedMethod1));
      Assert.That (registerableMethod2, Is.SameAs (expectedMethod2));
    }

    [Test]
    public void Test_ClosedGenericMethod_InClosedGenericType_HavingOverloadsDistinguishedByParameterName ()
    {
      var methodName = "GenericMethodOverloadedWithGenericParameterFromTypeAndDifferentParameterName";
      var method1 = typeof (GenericClass<int, string>).GetMethods()
          .Where (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "Int32")
          .Select (m => m.MakeGenericMethod (typeof (double)))
          .Single();

      var method2 = typeof (GenericClass<int, string>).GetMethods()
          .Where (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "String")
          .Select (m => m.MakeGenericMethod (typeof (double)))
          .Single();

      var expectedMethod1 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "T1");

      var expectedMethod2 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "T2");

      var registerableMethod1 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method1, true);
      var registerableMethod2 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method2, true);

      Assert.That (registerableMethod1, Is.SameAs (expectedMethod1));
      Assert.That (registerableMethod2, Is.SameAs (expectedMethod2));
    }

    [Test]
    public void Test_NonGenericMethod_InClosedGenericType_HavingOverloadsDistinguishedByParameterPosition ()
    {
      var methodName = "NonGenericMethodOverloadedWithGenericParameterFromTypeAtDifferentPosition";
      var method1 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "Int32");

      var method2 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "Double");

      var expectedMethod1 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "T1");

      var expectedMethod2 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "Double");

      var registerableMethod1 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method1, true);
      var registerableMethod2 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method2, true);

      Assert.That (registerableMethod1, Is.SameAs (expectedMethod1));
      Assert.That (registerableMethod2, Is.SameAs (expectedMethod2));
    }

    [Test]
    public void Test_ClosedGenericMethod_InClosedGenericType_HavingOverloadsDistinguishedByParameterPosition ()
    {
      var methodName = "GenericMethodOverloadedWithGenericParameterFromTypeAtDifferentPosition";
      var method1 = typeof (GenericClass<int, string>).GetMethods()
          .Where( m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "Int32")
          .Select (m => m.MakeGenericMethod (typeof (double)))
          .Single();

      var method2 = typeof (GenericClass<int, string>).GetMethods()
          .Where (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "T3")
          .Select (m => m.MakeGenericMethod (typeof (double)))
          .Single();

      var expectedMethod1 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "T1");

      var expectedMethod2 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "T3");

      var registerableMethod1 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method1, true);
      var registerableMethod2 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method2, true);

      Assert.That (registerableMethod1, Is.SameAs (expectedMethod1));
      Assert.That (registerableMethod2, Is.SameAs (expectedMethod2));
    }

    [Test]
    public void Test_NonGenericMethod_InClosedGenericType_HavingOverloadsDistinguishedByGenericParamterAndReturnType ()
    {
      var methodName = "NonGenericMethodOverloadedWithGenericParameterFromTypeAndDifferentReturnTypes";
      var method1 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "Int32");

      var method2 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "String");

      var expectedMethod1 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "T1");

      var expectedMethod2 = typeof (GenericClass<,>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "T2");

      var registerableMethod1 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method1, true);
      var registerableMethod2 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method2, true);

      Assert.That (registerableMethod1, Is.SameAs (expectedMethod1));
      Assert.That (registerableMethod2, Is.SameAs (expectedMethod2));
    }

    [Test]
    public void Test_NonGenericMethod_InClosedGenericType_HavingOverloadsDistinguishedByParameterTypeFromGenericClass_WithThrowOnAmbiguity_ThrowsNotSupportedException ()
    {
      var methodName = "NonGenericMethodOverloadedWithGenericParameterFromTypeAndSameParameterName";
      var method1 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "Int32");

      var method2 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "String");

      Assert.That (
          () => MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method1, throwOnAmbiguousMatch: true),
          Throws.TypeOf<NotSupportedException>().With.Message.StringStarting (
              "A generic method definition cannot be resolved for method 'Boolean NonGenericMethodOverloadedWithGenericParameterFromTypeAndSameParameterName(Int32, Double)' "
              + "on type 'Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders.MethodInfoBasedNodeTypeRegistryTests.TestDomain.GenericClass`2[T1,T2]' because a distinct match is not possible."));

      Assert.That (
          () => MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method2, throwOnAmbiguousMatch: true),
          Throws.TypeOf<NotSupportedException>().With.Message.StringStarting (
              "A generic method definition cannot be resolved for method 'Boolean NonGenericMethodOverloadedWithGenericParameterFromTypeAndSameParameterName(System.String, Double)' "
              + "on type 'Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders.MethodInfoBasedNodeTypeRegistryTests.TestDomain.GenericClass`2[T1,T2]' because a distinct match is not possible."));
    }

    [Test]
    public void Test_NonGenericMethod_InClosedGenericType_HavingOverloadsDistinguishedByParameterTypeFromGenericClass_WithDoNotThrowOnAmbiguity_ReturnsNull ()
    {
      var methodName = "NonGenericMethodOverloadedWithGenericParameterFromTypeAndSameParameterName";
      var method1 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "Int32");

      var method2 = typeof (GenericClass<int, string>).GetMethods()
          .Single (m => m.Name == methodName && m.GetParameters()[0].ParameterType.Name == "String");

      Assert.That (MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method1, throwOnAmbiguousMatch: false), Is.Null);
      Assert.That (MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method2, throwOnAmbiguousMatch: false), Is.Null);
    }
  }
}