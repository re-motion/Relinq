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

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Collections
{
  [TestFixture]
  public class MultiDictionaryTest
  {
    private MultiDictionary<string, string> _dictionary;
    private IDictionary<string, IList<string>> _castDictionary;

    private List<string> _list1;
    private List<string> _list2;

    private KeyValuePair<string, IList<string>> _item1;
    private KeyValuePair<string, IList<string>> _item2;

    [SetUp]
    public void SetUp ()
    {
      _dictionary = new MultiDictionary<string, string> ();
      _castDictionary = _dictionary;

      _list1 = new List<string> { "value1", "value2", "value3" };
      _list2 = new List<string> { "value4", "value5", "value6" };

      _item1 = new KeyValuePair<string, IList<string>> ("key1", _list1);
      _item2 = new KeyValuePair<string, IList<string>> ("key2", _list2);
    }

    [Test]
    public void Add_List ()
    {
      _dictionary.Add ("key1", _list1);
      Assert.That (_dictionary["key1"], Is.SameAs (_list1));
    }

    [Test]
    public void Add_Pair ()
    {
      _castDictionary.Add (new KeyValuePair<string, IList<string>> ("key1", _list1));
      Assert.That (_dictionary["key1"], Is.SameAs (_list1));
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
    public void GetEnumerator ()
    {
      _castDictionary.Add (_item1);
      _castDictionary.Add (_item2);

      var items = new List<KeyValuePair<string, IList<string>>> ();
      using (var enumerator = _dictionary.GetEnumerator ())
      {
        Assert.That (enumerator.MoveNext(), Is.True);
        items.Add (enumerator.Current);
        Assert.That (enumerator.MoveNext (), Is.True);
        items.Add (enumerator.Current);
        Assert.That (enumerator.MoveNext (), Is.False);
      }

      Assert.That (items, Is.EquivalentTo (new[] { _item1, _item2 }));
    }

    [Test]
    public void Contains ()
    {
      _castDictionary.Add (_item1);

      Assert.That (_castDictionary.Contains (_item1), Is.True);
      Assert.That (_castDictionary.Contains (_item2), Is.False);
    }

    [Test]
    public void ContainsKey_True ()
    {
      _dictionary.Add ("key1", _list1);
      Assert.That (_dictionary.ContainsKey ("key1"), Is.True);
    }

    [Test]
    public void ContainsKey_False ()
    {
      Assert.That (_dictionary.ContainsKey ("key1"), Is.False);
    }

    [Test]
    public void Clear ()
    {
      _dictionary.Add ("key1", _list1);
      Assert.That (_dictionary.ContainsKey ("key1"), Is.True);
      _dictionary.Clear ();
      Assert.That (_dictionary.ContainsKey ("key1"), Is.False);
    }

    [Test]
    public void Remove_Key ()
    {
      _castDictionary.Add (_item1);
      Assert.That (_dictionary.ContainsKey ("key1"), Is.True);

      var result = _dictionary.Remove ("key1");
      Assert.That (_dictionary.ContainsKey ("key1"), Is.False);
      Assert.That (result, Is.True);

      result = _dictionary.Remove ("key1");
      Assert.That (result, Is.False);
    }

    [Test]
    public void Remove_Item ()
    {
      _castDictionary.Add (_item1);
      Assert.That (_dictionary.ContainsKey ("key1"), Is.True);

      var result = _castDictionary.Remove (_item1);
      Assert.That (_dictionary.ContainsKey ("key1"), Is.False);
      Assert.That (result, Is.True);

      result = _castDictionary.Remove (_item1);
      Assert.That (result, Is.False);
    }

    [Test]
    public void KeyCount ()
    {
      _dictionary.Add ("key1", _list1);
      _dictionary.Add ("key2", _list1);
      Assert.That (_dictionary.KeyCount, Is.EqualTo (2));
    }

    [Test]
    public void CountValues ()
    {
      _dictionary.Add ("key1", _list1);
      _dictionary.Add ("key2", _list1);
      Assert.That (_dictionary.CountValues (), Is.EqualTo (6));
    }

    [Test]
    public void Count ()
    {
      Assert.That (_castDictionary.Count, Is.EqualTo (0));

      _dictionary.Add ("key1", "value1");
      Assert.That (_castDictionary.Count, Is.EqualTo (1));

      _dictionary.Add ("key1", "value2");
      Assert.That (_castDictionary.Count, Is.EqualTo (1));

      _dictionary.Add ("key2", "value3");
      Assert.That (_castDictionary.Count, Is.EqualTo (2));
    }

    [Test]
    public void IsReadOnly ()
    {
      Assert.That (_dictionary.IsReadOnly, Is.False);
    }

    [Test]
    public void TryGetValue ()
    {
      _dictionary.Add ("key1", _list1);

      IList<string> value;
      Assert.That(_dictionary.TryGetValue ("key1", out value), Is.True);
      
      Assert.That (value, Is.SameAs (_list1));
    }

    [Test]
    public void IndexGet ()
    {
      _dictionary.Add ("key1", _list1);

      Assert.That (_dictionary["key1"], Is.SameAs (_list1));
    }

    [Test]
    public void IndexGet_NewKey ()
    {
      Assert.That (_dictionary.KeyCount, Is.EqualTo (0));

      var result = _dictionary["key1"];
      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.Empty);

      Assert.That (_dictionary.KeyCount, Is.EqualTo (1));
    }

    [Test]
    public void IndexSet ()
    {
      _dictionary.Add ("key1", _list1);
      Assert.That (_dictionary["key1"], Is.SameAs (_list1));

      _dictionary["key1"] = _list2;
      Assert.That (_dictionary["key1"], Is.SameAs (_list2));
    }

    [Test]
    public void Keys ()
    {
      _dictionary.Add ("key1", _list1);
      _dictionary.Add ("key2", _list2);

      Assert.That (_dictionary.Keys, Is.EquivalentTo (new[] { "key1", "key2" }));
    }

    [Test]
    public void Values ()
    {
      _dictionary.Add ("key1", _list1);
      _dictionary.Add ("key2", _list2);

      Assert.That (_dictionary.Values, Is.EquivalentTo (new[] { _list1, _list2 }));
    }

    [Test]
    public void CopyTo ()
    {
      _castDictionary.Add (_item1);
      _castDictionary.Add (_item2);

      var array = new KeyValuePair<string, IList<string>>[4];
      _castDictionary.CopyTo (array, 1);

      Assert.That (array[0], Is.EqualTo (default (KeyValuePair<string, IList<string>>)));
      Assert.That (array[3], Is.EqualTo (default (KeyValuePair<string, IList<string>>)));

      Assert.That (array, List.Contains (_item1));
      Assert.That (array, List.Contains (_item2));
    }

  }
}