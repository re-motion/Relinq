// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.Linq.UnitTests.Linq.Core
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
    public void GetFieldOrPropertyType_Field ()
    {
      var memberInfo = typeof (DateTime).GetField ("MinValue");

      var type = ReflectionUtility.GetFieldOrPropertyType (memberInfo);
      Assert.That (type, Is.SameAs (typeof (DateTime)));
    }

    [Test]
    public void GetFieldOrPropertyType_Property ()
    {
      var memberInfo = typeof (DateTime).GetProperty ("Now");

      var type = ReflectionUtility.GetFieldOrPropertyType (memberInfo);
      Assert.That (type, Is.SameAs (typeof (DateTime)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "Argument must be FieldInfo or PropertyInfo.\r\nParameter name: fieldOrProperty")]
    public void GetFieldOrPropertyType_Other_Throws ()
    {
      var memberInfo = typeof (DateTime).GetMethod ("get_Now");

      ReflectionUtility.GetFieldOrPropertyType (memberInfo);
    }
  }
}
