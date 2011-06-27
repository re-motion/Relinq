// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using NUnit.Framework;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.Core.Utilities
{
  [TestFixture]
  public class HashCodeUtilityTest
  {
    [Test]
    public void GetHashCodeOrZero_Null ()
    {
      var result = HashCodeUtility.GetHashCodeOrZero (null);

      Assert.That (result, Is.EqualTo (0));
    }

    [Test]
    public void GetHashCodeOrZero_NonNull ()
    {
      var value = new object ();
      var result = HashCodeUtility.GetHashCodeOrZero (value);

      Assert.That (result, Is.EqualTo (value.GetHashCode()));
    }

    [Test]
    public void GetHashCodeForSequence_Empty ()
    {
      var sequence = new int[0];
      var result = HashCodeUtility.GetHashCodeForSequence (sequence);

      Assert.That (result, Is.EqualTo (0));
    }

    [Test]
    public void GetHashCodeForSequence_NonEmpty ()
    {
      var sequence = new[] { 1, 2, 3 };
      var result = HashCodeUtility.GetHashCodeForSequence (sequence);

      Assert.That (result, Is.EqualTo (1.GetHashCode() ^ 2.GetHashCode() ^ 3.GetHashCode()));
    }

    [Test]
    public void GetHashCodeForSequence_NullElements ()
    {
      var sequence = new object[] { 1, 0, 3 };
      var result = HashCodeUtility.GetHashCodeForSequence (sequence);

      Assert.That (result, Is.EqualTo (1.GetHashCode () ^ 3.GetHashCode ()));
    }
  }
}