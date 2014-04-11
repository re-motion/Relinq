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
using System.Collections.ObjectModel;
using NUnit.Framework;
using Remotion.Linq.Collections;

namespace Remotion.Linq.UnitTests.Collections
{
  [TestFixture]
  public class ObservableCollectionExtensionsTest
  {
    private ObservableCollection<int> _collection;

    [SetUp]
    public void SetUp ()
    {
      _collection = new ObservableCollection<int>();
    }

    [Test]
    public void AsChangeResistantEnumerable ()
    {
      using (var enumerator = _collection.AsChangeResistantEnumerable ().GetEnumerator ())
      {
        Assert.That (enumerator, Is.InstanceOf (typeof (ChangeResistantObservableCollectionEnumerator<int>)));
      }
    }

    [Test]
    public void AsChangeResistantEnumerableWithIndex ()
    {
      _collection.Add (100);
      _collection.Add (200);

      using (var enumerator = _collection.AsChangeResistantEnumerableWithIndex ().GetEnumerator ())
      {
        Assert.That (enumerator.MoveNext (), Is.True);
        Assert.That (enumerator.Current.Index, Is.EqualTo (0));
        Assert.That (enumerator.Current.Value, Is.EqualTo (100));
        
        Assert.That (enumerator.MoveNext (), Is.True);
        Assert.That (enumerator.Current.Index, Is.EqualTo (1));
        Assert.That (enumerator.Current.Value, Is.EqualTo (200));

        Assert.That (enumerator.MoveNext (), Is.False);
      }
    }

    [Test]
    public void AsChangeResistantEnumerableWithIndex_IndexAdaptsToChanges ()
    {
      _collection.Add (100);
      _collection.Add (200);

      using (var enumerator = _collection.AsChangeResistantEnumerableWithIndex ().GetEnumerator ())
      {
        Assert.That (enumerator.MoveNext (), Is.True);
        Assert.That (enumerator.MoveNext (), Is.True);

        Assert.That (enumerator.Current.Index, Is.EqualTo (1));
        Assert.That (enumerator.Current.Value, Is.EqualTo (200));

        _collection.Insert (0, 0);
        Assert.That (enumerator.Current.Index, Is.EqualTo (2));
        Assert.That (enumerator.Current.Value, Is.EqualTo (200));
      }
    }
  }
}