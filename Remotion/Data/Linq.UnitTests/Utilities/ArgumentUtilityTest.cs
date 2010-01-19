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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.Utilities
{
  [TestFixture]
  public class ArgumentUtilityTest
  {
    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void CheckNotNull_Nullable_Fail()
    {
      ArgumentUtility.CheckNotNull ("arg", (int?) null);
    }

    [Test]
    public void CheckNotNull_Nullable_Succeed()
    {
      int? result = ArgumentUtility.CheckNotNull ("arg", (int?) 1);
      Assert.That (result, Is.EqualTo (1));
    }

    [Test]
    public void CheckNotNull_Value_Succeed()
    {
      int result = ArgumentUtility.CheckNotNull ("arg", 1);
      Assert.That (result, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void CheckNotNull_Reference_Fail()
    {
      ArgumentUtility.CheckNotNull ("arg", (string) null);
    }

    [Test]
    public void CheckNotNull_Reference_Succeed()
    {
      string result = ArgumentUtility.CheckNotNull ("arg", string.Empty);
      Assert.That (result, Is.SameAs (string.Empty));
    }

    [Test]
    public void CheckNotNullAndType_Succeed_Int ()
    {
      int result = ArgumentUtility.CheckNotNullAndType<int> ("arg", 1);
      Assert.AreEqual (1, result);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void CheckNotNullAndType_Fail_Int_Null ()
    {
      ArgumentUtility.CheckNotNullAndType<int> ("arg", null);
    }

    [Test]
    public void CheckNotNullAndType_Succeed_Int_NullableInt ()
    {
      int result = ArgumentUtility.CheckNotNullAndType<int> ("arg", (int?) 1);
      Assert.AreEqual (1, result);
    }

    [Test]
    public void CheckNotNullAndType_Succeed_NullableInt ()
    {
      int? result = ArgumentUtility.CheckNotNullAndType<int?> ("arg", (int?) 1);
      Assert.AreEqual (1, result);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void CheckNotNullAndType_Fail_NullableInt_Null ()
    {
      ArgumentUtility.CheckNotNullAndType<int?> ("arg", null);
    }

    [Test]
    public void CheckNotNullAndType_Succeed_NullableInt_Int ()
    {
      int? result = ArgumentUtility.CheckNotNullAndType<int?> ("arg", 1);
      Assert.AreEqual (1, result);
    }

    [Test]
    public void CheckNotNullAndType_Succeed_String ()
    {
      string result = ArgumentUtility.CheckNotNullAndType<string> ("arg", "test");
      Assert.AreEqual ("test", result);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void CheckNotNullAndType_Fail_StringNull ()
    {
      ArgumentUtility.CheckNotNullAndType<string> ("arg", null);
    }

    [Test]
    public void CheckNotNullAndType_Succeed_Object_String ()
    {
      object result = ArgumentUtility.CheckNotNullAndType<object> ("arg", "test");
      Assert.AreEqual ("test", result);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void CheckNotNullAndType_Fail_String_Int ()
    {
      ArgumentUtility.CheckNotNullAndType<string> ("arg", 1);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void CheckNotNullAndType_Fail_Long_Int ()
    {
      ArgumentUtility.CheckNotNullAndType<long> ("arg", 1);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void CheckNotNullAndType_Fail_Int_String ()
    {
      ArgumentUtility.CheckNotNullAndType<int> ("arg", "test");
    }

    [Test]
    [ExpectedExceptionAttribute (typeof (ArgumentNullException))]
    public void CheckNotNullOrEmpty_Fail_NullString ()
    {
      const string value = null;
      ArgumentUtility.CheckNotNullOrEmpty ("arg", value);
    }

    [Test]
    [ExpectedExceptionAttribute (typeof (ArgumentEmptyException))]
    public void CheckNotNullOrEmpty_Fail_EmptyString ()
    {
      ArgumentUtility.CheckNotNullOrEmpty ("arg", "");
    }

    [Test]
    public void CheckNotNullOrEmpty_Succeed_String ()
    {
      string result = ArgumentUtility.CheckNotNullOrEmpty ("arg", "Test");
      Assert.That (result, Is.EqualTo ("Test"));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void CheckTypeIsAssignableFrom_Fail ()
    {
      ArgumentUtility.CheckTypeIsAssignableFrom ("arg", typeof (object), typeof (string));
    }

    [Test]
    public void CheckTypeIsAssignableFrom_Succeed_Null ()
    {
      Type result = ArgumentUtility.CheckTypeIsAssignableFrom ("arg", null, typeof (object));
      Assert.That (result, Is.Null);
    }

    [Test]
    public void CheckTypeIsAssignableFrom_Succeed ()
    {
      Type result = ArgumentUtility.CheckTypeIsAssignableFrom ("arg", typeof (string), typeof (object));
      Assert.That (result, Is.SameAs (typeof (string)));
    }
  }
}