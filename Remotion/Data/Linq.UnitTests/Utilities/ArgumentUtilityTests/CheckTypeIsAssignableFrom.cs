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

namespace Remotion.Data.Linq.UnitTests.Utilities.ArgumentUtilityTests
{
	[TestFixture]
	public class CheckTypeIsAssignableFrom
	{
    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Fail ()
    {
      ArgumentUtility.CheckTypeIsAssignableFrom ("arg", typeof (object), typeof (string));
    }
    [Test]
    public void Succeed_Null ()
    {
      Type result = ArgumentUtility.CheckTypeIsAssignableFrom ("arg", null, typeof (object));
      Assert.That (result, Is.Null);
    }

	  [Test]
    public void Succeed ()
    {
      Type result = ArgumentUtility.CheckTypeIsAssignableFrom ("arg", typeof (string), typeof (object));
      Assert.That (result, Is.SameAs (typeof (string)));
    }
	}
}
