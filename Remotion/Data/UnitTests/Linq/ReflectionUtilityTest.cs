// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Remotion.Data.UnitTests.Linq
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
    public void TryGetItemTypeOfIEnumerable_InvalidType ()
    {
      Assert.That (ReflectionUtility.TryGetItemTypeOfIEnumerable (typeof (int)), Is.Null);
    }
  }
}