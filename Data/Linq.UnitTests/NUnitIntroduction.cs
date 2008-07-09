/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class NUnitIntroduction
  {
    private object _instance;

    [SetUp]
    public void SetUp ()
    {
      _instance = new object ();
    }

    [TearDown]
    public void TearDown ()
    {
      _instance = null;
    }

    [Test]
    public void Object_Equals ()
    {
      Assert.IsTrue (_instance.Equals (_instance));
      Assert.AreNotEqual (1, _instance);
    }

    [Test]
    public void Object_ToString ()
    {
      Assert.AreEqual (typeof (Object).FullName, _instance.ToString ());
    }

    [Test]
    [Ignore ("TODO")]
    public void TodoFeature ()
    {
      Assert.Fail ();
    }

    [Test]
    public void MoreComplexTests ()
    {
      List<int> myList2 = new List<int> ();
      myList2.Add (1);
      myList2.Add (1);
      myList2.Add (1);

      Assert.That (myList2, List.All.EqualTo (1));
    }
  }
}
