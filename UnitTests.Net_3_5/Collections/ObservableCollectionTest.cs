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
using Remotion.Linq.Collections;

namespace Remotion.Linq.UnitTests.Collections
{
  [TestFixture]
  public class ObservableCollectionTest
  {
    private ObservableCollection<int> _collection;

    [SetUp]
    public void SetUp ()
    {
      _collection = new ObservableCollection<int>();
    }

    [Test]
    public void ItemsCleared ()
    {
      bool eventRaised = false;
      NotifyCollectionChangedEventArgs eventArgs = null;
      _collection.CollectionChanged += (sender, e) => { eventRaised = true; eventArgs = e; };
      
      _collection.Clear();
      
      Assert.That (eventRaised, Is.True);
      Assert.That (eventArgs.Action, Is.EqualTo (NotifyCollectionChangedAction.Reset));
      Assert.That (eventArgs.NewStartingIndex, Is.EqualTo (-1));
      Assert.That (eventArgs.OldStartingIndex, Is.EqualTo (-1));
      Assert.That (eventArgs.NewItems, Is.Null);
      Assert.That (eventArgs.OldItems, Is.Null);
    }

    [Test]
    public void ItemsCleared_TriggeredAfterOperation ()
    {
      _collection.Add (7);

      _collection.CollectionChanged += (sender, e) => Assert.That (_collection, Is.Empty);

      _collection.Clear();
    }

    [Test]
    public void Clear_WithoutSubscription ()
    {
      _collection.Clear ();
    }

    [Test]
    public void ItemInserted ()
    {
      _collection.Add (7);

      bool eventRaised = false;
      NotifyCollectionChangedEventArgs eventArgs = null;
      _collection.CollectionChanged += ((sender, e) => { eventRaised = true; eventArgs = e; });
      
      _collection.Add (8);

      Assert.That (eventRaised, Is.True);
      Assert.That (eventArgs.Action, Is.EqualTo (NotifyCollectionChangedAction.Add));
      Assert.That (eventArgs.NewStartingIndex, Is.EqualTo (1));
      Assert.That (eventArgs.OldStartingIndex, Is.EqualTo (-1));
      Assert.That (eventArgs.NewItems, Is.EqualTo (new[] { 8 }));
      Assert.That (eventArgs.OldItems, Is.Null);
    }

    [Test]
    public void ItemInserted_TriggeredAfterOperation ()
    {
      _collection.Add (7);

      _collection.CollectionChanged += (sender, e) => Assert.That (_collection, Has.Member (8));

      _collection.Add (8);
    }

    [Test]
    public void Insert_WithoutSubscription ()
    {
      _collection.Add (1);
    }

    [Test]
    public void ItemRemoved ()
    {
      _collection.Add (6);
      _collection.Add (7);

      bool eventRaised = false;
      NotifyCollectionChangedEventArgs eventArgs = null;
      _collection.CollectionChanged += ((sender, e) => { eventRaised = true; eventArgs = e; });

      _collection.RemoveAt (1);

      Assert.That (eventRaised, Is.True);
      Assert.That (eventArgs.Action, Is.EqualTo (NotifyCollectionChangedAction.Remove));
      Assert.That (eventArgs.NewStartingIndex, Is.EqualTo (-1));
      Assert.That (eventArgs.OldStartingIndex, Is.EqualTo (1));
      Assert.That (eventArgs.NewItems, Is.Null);
      Assert.That (eventArgs.OldItems, Is.EqualTo (new[] { 7 }));
    }

    [Test]
    public void ItemRemoved_TriggeredAfterOperation ()
    {
      _collection.Add (7);

      _collection.CollectionChanged += (sender, e) => Assert.That (_collection, Has.No.Contains (7));

      _collection.Remove (7);
    }

    [Test]
    public void Remove_WithoutSubscription ()
    {
      _collection.Add (1);
      _collection.RemoveAt (0);
    }

    [Test]
    public void ItemSet ()
    {
      _collection.Add (6);
      _collection.Add (7);

      bool eventRaised = false;
      NotifyCollectionChangedEventArgs eventArgs = null;
      _collection.CollectionChanged += ((sender, e) =>
      {
        eventRaised = true;
        eventArgs = e;
      });

      _collection[1] = 8;

      Assert.That (eventRaised, Is.True);
      Assert.That (eventArgs.Action, Is.EqualTo (NotifyCollectionChangedAction.Replace));
      Assert.That (eventArgs.NewStartingIndex, Is.EqualTo (1));
      Assert.That (eventArgs.OldStartingIndex, Is.EqualTo (1));
      Assert.That (eventArgs.NewItems, Is.EqualTo (new[] { 8 }));
      Assert.That (eventArgs.OldItems, Is.EqualTo (new[] { 7 }));
    }

    [Test]
    public void ItemSet_TriggeredAfterOperation ()
    {
      _collection.Add (7);

      _collection.CollectionChanged += (sender, e) => Assert.That (_collection, Has.Member (8));

      _collection[0] = 8;
    }

    [Test]
    public void Set_WithoutSubscription ()
    {
      _collection.Add (1);
      _collection[0] = 8;
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