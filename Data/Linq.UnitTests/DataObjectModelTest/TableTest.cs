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
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class TableTest
  {
    [Test]
    public void Equal()
    {
      Table t1 = new Table ("x", "y");
      Table t2 = new Table ("x", "y");

      Assert.AreEqual (t1, t2);
    }

    [Test]
    public void NotEqual ()
    {
      Table t1 = new Table ("x", "y");
      Table t2 = new Table ("y", "z");

      Assert.AreNotEqual (t1, t2);
    }

    [Test]
    public void EqualHashCode ()
    {
      Table t1 = new Table ("x", "y");
      Table t2 = new Table ("x", "y");

      int hashCode1 = t1.GetHashCode();
      int hashCode2 = t2.GetHashCode();
      Assert.AreEqual (hashCode1, hashCode2);
    }

    [Test]
    public void NotEqualHashCode ()
    {
      Table t1 = new Table ("x", "y");
      Table t2 = new Table ("y", "z");

      int hashCode1 = t1.GetHashCode ();
      int hashCode2 = t2.GetHashCode ();
      Assert.AreNotEqual (hashCode1, hashCode2);
    }

    [Test]
    public void SetAlias ()
    {
      Table t = new Table ("x", null);
      Assert.IsNull (t.Alias);
      t.SetAlias ("hugo");
      Assert.AreEqual ("hugo", t.Alias);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Table 'x' already has alias 'y', new alias 'hugo' cannot be set.")]
    public void SetAlias_NonNullAlias ()
    {
      Table t = new Table ("x", "y");
      t.SetAlias ("hugo");
    }
  }
}
