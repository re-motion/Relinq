// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using NUnit.Framework;
using Remotion.Linq;

namespace Remotion.Linq.UnitTests
{
  [TestFixture]
  public class UniqueIdentifierGeneratorTest
  {
    private UniqueIdentifierGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator();
    }

    [Test]
    public void GetUniqueIdentifier ()
    {
      var identifier = _generator.GetUniqueIdentifier ("x");
      Assert.That (identifier, Is.EqualTo ("x0"));
    }

    [Test]
    public void GetUniqueIdentifier_Twice ()
    {
      var identifier1 = _generator.GetUniqueIdentifier ("x");
      var identifier2 = _generator.GetUniqueIdentifier ("x");
      Assert.That (identifier1, Is.EqualTo ("x0"));
      Assert.That (identifier2, Is.EqualTo ("x1"));
    }

    [Test]
    public void AddKnownIdentifier ()
    {
      _generator.AddKnownIdentifier ("x1");

      var identifier1 = _generator.GetUniqueIdentifier ("x");
      var identifier2 = _generator.GetUniqueIdentifier ("x");
      var identifier3 = _generator.GetUniqueIdentifier ("x");

      Assert.That (identifier1, Is.EqualTo ("x0"));
      Assert.That (identifier2, Is.EqualTo ("x2"));
      Assert.That (identifier3, Is.EqualTo ("x3"));
    }

    [Test]
    public void Reset_ResetsCounter ()
    {
      _generator.GetUniqueIdentifier ("x");
      _generator.GetUniqueIdentifier ("x");
      _generator.GetUniqueIdentifier ("x");

      _generator.Reset ();

      var identifier1 = _generator.GetUniqueIdentifier ("x");
      var identifier2 = _generator.GetUniqueIdentifier ("x");
      var identifier3 = _generator.GetUniqueIdentifier ("x");
      
      Assert.That (identifier1, Is.EqualTo ("x0"));
      Assert.That (identifier2, Is.EqualTo ("x1"));
      Assert.That (identifier3, Is.EqualTo ("x2"));
    }

    [Test]
    public void Reset_ResetsKnownIdentifiers ()
    {
      _generator.AddKnownIdentifier ("x1");

      _generator.Reset ();

      var identifier1 = _generator.GetUniqueIdentifier ("x");
      var identifier2 = _generator.GetUniqueIdentifier ("x");
      var identifier3 = _generator.GetUniqueIdentifier ("x");

      Assert.That (identifier1, Is.EqualTo ("x0"));
      Assert.That (identifier2, Is.EqualTo ("x1"));
      Assert.That (identifier3, Is.EqualTo ("x2"));
    }
  }
}
