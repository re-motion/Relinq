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
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Collections;

namespace Remotion.Data.Linq.UnitTests.Collections
{
  [TestFixture]
  public class MultiDictionaryTest
  {
    MultiDictionary<string, string> _dictionary;

    [SetUp]
    public void SetUp ()
    {
      _dictionary = new MultiDictionary<string, string> ();
    }

    [Test]
    public void Add ()
    {
      _dictionary.Add ("key1", new List<string> { "value1", "value2", "value3" });
      Assert.That (_dictionary["key1"].Count, Is.EqualTo (3));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void AddNullValues ()
    {
      _dictionary.Add ("key1", null);
    }

    [Test]
    public void Clear ()
    {
      _dictionary.Add ("key1", new List<string> { "value1", "value2", "value3" });
      Assert.That (_dictionary["key1"].Count, Is.EqualTo (3));
      _dictionary.Clear();
      Assert.That (_dictionary["key1"].Count, Is.EqualTo (0));
    }

    [Test]
    public void Contains ()
    {
      var item1 = new KeyValuePair<string, IList<string>> ("key1", new List<string> { "value1", "value2", "value3" });
      var item2 = new KeyValuePair<string, IList<string>> ("key2", new List<string> { "value1", "value2", "value3" });
      _dictionary.Add (item1);
      Assert.That (_dictionary.Contains (item1), Is.True);
      Assert.That (_dictionary.Contains (item2), Is.False);
    }

    [Test]
    public void Remove ()
    {
      var item1 = new KeyValuePair<string, IList<string>> ("key1", new List<string> { "value1", "value2", "value3" });
      _dictionary.Add (item1);
      Assert.That (_dictionary["key1"].Count, Is.EqualTo (3));
      _dictionary.Remove ("key1");
      Assert.That (_dictionary["key1"].Count, Is.EqualTo (0));
    }
  }
}