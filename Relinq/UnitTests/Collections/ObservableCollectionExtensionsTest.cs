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