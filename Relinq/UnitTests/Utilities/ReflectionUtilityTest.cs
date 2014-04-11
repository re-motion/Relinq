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
using System.Linq.Expressions;
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
    public void GetItemTypeOfIEnumerable_ArgumentImplementsIEnumerable ()
    {
      Assert.That (ReflectionUtility.GetItemTypeOfIEnumerable (typeof (List<int>), "x"), Is.SameAs (typeof (int)));
    }

    [Test]
    public void GetItemTypeOfIEnumerable_ArgumentIsIEnumerable ()
    {
      Assert.That (ReflectionUtility.GetItemTypeOfIEnumerable (typeof (IEnumerable<int>), "x"), Is.SameAs (typeof (int)));
      Assert.That (ReflectionUtility.GetItemTypeOfIEnumerable (typeof (IEnumerable<IEnumerable<string>>), "x"), Is.SameAs (typeof (IEnumerable<string>)));
    }

    [Test]
    [ExpectedException (ExpectedMessage = "Expected a type implementing IEnumerable<T>, but found 'System.Int32'.\r\nParameter name: x")]
    public void GetItemTypeOfIEnumerable_InvalidType ()
    {
      ReflectionUtility.GetItemTypeOfIEnumerable (typeof (int), "x");
    }

    [Test]
    public void TryGetItemTypeOfIEnumerable_ArgumentIsArray ()
    {
      Assert.That (ReflectionUtility.TryGetItemTypeOfIEnumerable (typeof (int[])), Is.SameAs (typeof (int)));
      Assert.That (ReflectionUtility.TryGetItemTypeOfIEnumerable (typeof (double[])), Is.SameAs (typeof (double)));
    }

    [Test]
    public void TryGetItemTypeOfIEnumerable_ArgumentIsArray_Strange ()
    {
      Expression<Func<int, IEnumerable<double>>> collectionSelector = x => new double[1];
      Assert.That (ReflectionUtility.TryGetItemTypeOfIEnumerable (collectionSelector.Body.Type), Is.SameAs (typeof (double)));
    }

    [Test]
    public void TryGetItemTypeOfIEnumerable_ArgumentImplementsIEnumerable ()
    {
      Assert.That (ReflectionUtility.TryGetItemTypeOfIEnumerable (typeof (List<int>)), Is.SameAs (typeof (int)));
    }

    [Test]
    public void TryGetItemTypeOfIEnumerable_ArgumentIsIEnumerable ()
    {
      Assert.That (ReflectionUtility.TryGetItemTypeOfIEnumerable (typeof (IEnumerable<int>)), Is.SameAs (typeof (int)));
      Assert.That (ReflectionUtility.TryGetItemTypeOfIEnumerable (typeof (IEnumerable<IEnumerable<string>>)), Is.SameAs (typeof (IEnumerable<string>)));
    }

    [Test]
    public void TryGetItemTypeOfIEnumerable_ArgumentImplementsIEnumerable_NonGeneric ()
    {
      Assert.That (ReflectionUtility.TryGetItemTypeOfIEnumerable (typeof (ArrayList)), Is.SameAs (typeof (object)));
    }

    [Test]
    public void TryGetItemTypeOfIEnumerable_ArgumentImplementsIEnumerable_BothGenericAndNonGeneric ()
    {
      Assert.That (ReflectionUtility.TryGetItemTypeOfIEnumerable (typeof (int[])), Is.SameAs (typeof (int)));
    }

    [Test]
    public void TryGetItemTypeOfIEnumerable_InvalidType ()
    {
      Assert.That (ReflectionUtility.TryGetItemTypeOfIEnumerable (typeof (int)), Is.Null);
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
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "Argument must be FieldInfo, PropertyInfo, or MethodInfo.\r\nParameter name: member")]
    public void GetMemberReturnType_Other_Throws ()
    {
      var memberInfo = typeof (DateTime);

      ReflectionUtility.GetMemberReturnType (memberInfo);
    }
  }
}
