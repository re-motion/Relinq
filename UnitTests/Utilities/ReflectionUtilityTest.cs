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
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Utilities
{
  [TestFixture]
  public class ReflectionUtilityTest
  {
    [Test]
    public void GetMethod ()
    {
      MethodInfo method = ReflectionUtility.GetMethod (() => "x".ToUpper());
      Assert.That (method, Is.EqualTo (typeof (string).GetMethod ("ToUpper", new Type[0])));
    }

    [Test]
    public void GetMethod_PropertyAccess ()
    {
      MethodInfo method = ReflectionUtility.GetMethod (() => "x".Length);
      Assert.That (method, Is.EqualTo (typeof (string).GetMethod ("get_Length", new Type[0])));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void GetMethod_InvalidExpression ()
    {
      ReflectionUtility.GetMethod (() => "x");
    }

    [Test]
    public void GetRuntimeMethodChecked_ExistingMethod_ReturnsMethodInfo ()
    {
      MethodInfo method = ReflectionUtility.GetRuntimeMethodChecked (typeof (string), "Substring", new[] { typeof (int) });
      Assert.That (method, Is.EqualTo (typeof (string).GetMethod ("Substring", new[] { typeof (int) })));
    }

    [Test]
    public void GetRuntimeMethodChecked_NonExistingMethod_ThrowsInvalidOperationException ()
    {
      Assert.That (
          () => ReflectionUtility.GetRuntimeMethodChecked (typeof (string), "Substring", new[] { typeof (double) }),
          Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo ("Method 'Substring (Double)' was not found on type 'System.String'"));
    }

    [Test]
    public void CheckTypeIsClosedGenericIEnumerable_ImplementsIEnumerable_DoesNotThrow()
    {
      Assert.That (()=>ReflectionUtility.CheckTypeIsClosedGenericIEnumerable (typeof (List<int>), "x"), Throws.Nothing);
    }

    [Test]
    public void CheckTypeIsClosedGenericIEnumerable_DoesNotImplementsIEnumerable_ThrowsArgumentException ()
    {
      Assert.That (
          () => ReflectionUtility.CheckTypeIsClosedGenericIEnumerable (typeof (int), "x"),
          Throws.ArgumentException.With.Message.EqualTo (
              "Expected a closed generic type implementing IEnumerable<T>, but found 'System.Int32'.\r\nParameter name: x"));
    }

    [Test]
    public void CheckTypeIsClosedGenericIEnumerable_OpenIEnumerable_ThrowsArgumentException ()
    {
      Assert.That (
          () => ReflectionUtility.CheckTypeIsClosedGenericIEnumerable (typeof (List<>), "x"),
          Throws.ArgumentException.With.Message.EqualTo (
              "Expected a closed generic type implementing IEnumerable<T>, but found 'System.Collections.Generic.List`1[T]'.\r\nParameter name: x"));
    }

    [Test]
    public void GetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerable ()
    {
      Assert.That (ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (typeof (List<int>), "x"), Is.SameAs (typeof (int)));
    }

    [Test]
    public void GetItemTypeOfClosedGenericIEnumerable_ArgumentIsIEnumerable ()
    {
      Assert.That (ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (typeof (IEnumerable<int>), "x"), Is.SameAs (typeof (int)));
      Assert.That (ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (typeof (IEnumerable<IEnumerable<string>>), "x"), Is.SameAs (typeof (IEnumerable<string>)));
    }

    [Test]
    public void GetItemTypeOfClosedGenericIEnumerable_NonGenericIEnumerable_ThrowsArgumentException ()
    {
      Assert.That (
          () => ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (typeof (ArrayList), "x"),
          Throws.ArgumentException.With.Message.EqualTo (
              "Expected a closed generic type implementing IEnumerable<T>, but found 'System.Collections.ArrayList'.\r\nParameter name: x"));
    }

    [Test]
    public void GetItemTypeOfClosedGenericIEnumerable_InvalidType_ThrowsArgumentException ()
    {
      Assert.That (
          () => ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (typeof (int), "x"),
          Throws.ArgumentException.With.Message.EqualTo (
              "Expected a closed generic type implementing IEnumerable<T>, but found 'System.Int32'.\r\nParameter name: x"));
    }

    [Test]
    public void GetMemberReturnType_Field ()
    {
      var memberInfo = typeof (DateTime).GetField ("MinValue");

      var type = ReflectionUtility.GetMemberReturnType (memberInfo);
      Assert.That (type, Is.SameAs (typeof (DateTime)));
    }

    [Test]
    public void GetMemberReturnType_Property ()
    {
      var memberInfo = typeof (DateTime).GetProperty ("Now");

      var type = ReflectionUtility.GetMemberReturnType (memberInfo);
      Assert.That (type, Is.SameAs (typeof (DateTime)));
    }

    [Test]
    public void GetMemberReturnType_Method ()
    {
      var memberInfo = typeof (DateTime).GetMethod ("get_Now");

      var type = ReflectionUtility.GetMemberReturnType (memberInfo);
      Assert.That (type, Is.SameAs (typeof (DateTime)));
    }

    [Test]
    public void GetMemberReturnType_Other_Throws ()
    {
      var memberInfo = typeof (DateTime);

      Assert.That (
          () => ReflectionUtility.GetMemberReturnType (memberInfo),
          Throws.ArgumentException.With.Message.EqualTo ("Argument must be FieldInfo, PropertyInfo, or MethodInfo.\r\nParameter name: member"));
    }
  }
}
