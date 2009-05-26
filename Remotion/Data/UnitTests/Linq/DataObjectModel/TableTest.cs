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
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.UnitTests.Linq.DataObjectModelTest
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
