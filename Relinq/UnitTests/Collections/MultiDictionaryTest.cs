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

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Linq.Collections;

namespace Remotion.Linq.UnitTests.Collections
{
  [TestFixture]
  public class MultiDictionaryExtensionsTest
  {
    private IDictionary<string, ICollection<string>> _dictionary;

    private List<string> _list1;

    [SetUp]
    public void SetUp ()
    {
      _dictionary = new Dictionary<string, ICollection<string>> ();

      _list1 = new List<string> { "value1", "value2", "value3" };
    }

    [Test]
    public void Add_Value ()
    {
      _dictionary.Add ("key1", "value1");
      _dictionary.Add ("key1", "value2");
      _dictionary.Add ("key2", "value3");

      Assert.That (_dictionary["key1"], Is.EqualTo (new[] { "value1", "value2" }));
      Assert.That (_dictionary["key2"], Is.EqualTo (new[] { "value3" }));
    }

    [Test]
    public void CountValues ()
    {
      _dictionary.Add ("key1", _list1);
      _dictionary.Add ("key2", _list1);
      Assert.That (_dictionary.CountValues (), Is.EqualTo (6));
    }
  }
}