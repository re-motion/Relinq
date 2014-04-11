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
using NUnit.Framework;

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
