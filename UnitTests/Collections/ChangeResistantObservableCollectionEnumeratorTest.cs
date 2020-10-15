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
#if !NET_3_5
using System.Collections.ObjectModel;
#endif
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Linq.Collections;

namespace Remotion.Linq.UnitTests.Collections
{
  [TestFixture]
  public class ChangeResistantObservableCollectionEnumeratorTest
  {
    private ObservableCollection<int> _collection;
    private ChangeResistantObservableCollectionEnumerator<int> _enumerator;

    [SetUp]
    public void SetUp ()
    {
      _collection = new ObservableCollection<int> { 0, 1, 2, 3, 4 };
      _enumerator = new ChangeResistantObservableCollectionEnumerator<int> (_collection);
    }

    [Test]
    public void Current_AfterMoveNext ()
    {
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (0));
    }

    [Test]
    public void Current_WithoutMoveNext_ThrowsInvalidOperationException ()
    {
      Assert.That (() => _enumerator.Current, Throws.InvalidOperationException);
    }

    [Test]
    public void Current_AfterLastMoveNext_ThrowsInvalidOperationException ()
    {
      while (_enumerator.MoveNext())
      {
      }

      Assert.That (() => _enumerator.Current, Throws.InvalidOperationException);
    }

    [Test]
    public void Current_AfterReset_ThrowsInvalidOperationException ()
    {
      _enumerator.MoveNext();
      _enumerator.Reset();
      Assert.That (() => _enumerator.Current, Throws.InvalidOperationException);
    }

    [Test]
    public void Current_AfterDispose_ThrowsObjectDisposedException ()
    {
      _enumerator.Dispose();
      Assert.That (() => _enumerator.Current, Throws.Exception.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void Index_AfterDispose_ThrowsObjectDisposedException ()
    {
      _enumerator.Dispose();
      Assert.That (() => _enumerator.Index, Throws.Exception.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void MoveNext_IncrementsPosition ()
    {
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (0));
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (1));
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (2));
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (3));
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (4));
    }

    [Test]
    public void MoveNext_ReturnValue ()
    {
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.MoveNext(), Is.False);
      Assert.That (_enumerator.MoveNext(), Is.False);
    }

    [Test]
    public void MoveNext_AfterDispose ()
    {
      _enumerator.Dispose();
      Assert.That (
          () => _enumerator.MoveNext(),
          Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public void Reset ()
    {
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (0));
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _enumerator.Reset();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (0));
    }

    [Test]
    public void Reset_AfterDispose ()
    {
      _enumerator.Dispose();
      Assert.That (
          () => _enumerator.Reset(),
          Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public void CollectionInsert_AfterCurrent_LeavesIndex ()
    {
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (0));

      _collection.Insert (1, 100);
      Assert.That (_enumerator.Index, Is.EqualTo (0));
    }

    [Test]
    public void CollectionInsert_AtCurrent_IncrementsIndex ()
    {
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (0));

      _collection.Insert (0, 100);
      Assert.That (_enumerator.Index, Is.EqualTo (1));
    }

    [Test]
    public void CollectionInsert_BeforeCurrent_IncrementsIndex ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (1));

      _collection.Insert (0, 100);
      Assert.That (_enumerator.Index, Is.EqualTo (2));
    }

    [Test]
    public void CollectionRemove_AfterCurrent_LeavesIndex ()
    {
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (0));

      _collection.RemoveAt (1);
      Assert.That (_enumerator.Index, Is.EqualTo (0));
    }

    [Test]
    public void CollectionRemove_AtCurrent_DecrementsIndex ()
    {
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (0));

      _collection.RemoveAt (0);
      Assert.That (_enumerator.Index, Is.EqualTo (-1));
    }

    [Test]
    public void CollectionRemove_BeforeCurrent_DecrementsIndex ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (1));

      _collection.RemoveAt (0);
      Assert.That (_enumerator.Index, Is.EqualTo (0));
    }

    [Test]
    public void CollectionReplace_AfterCurrent_LeavesIndex ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (1));

      _collection[2] = 100;
      Assert.That (_enumerator.Index, Is.EqualTo (1));
    }

    [Test]
    public void CollectioReplace_AtCurrent_LeavesIndex ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (1));

      _collection[1] = 100;
      Assert.That (_enumerator.Index, Is.EqualTo (1));
    }

    [Test]
    public void CollectionReplace_BeforeCurrent_LeavesIndex ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (1));

      _collection[0] = 100;
      Assert.That (_enumerator.Index, Is.EqualTo (1));
    }

#if !NET_3_5
    [Test]
    public void CollectionMove_AfterCurrentToBeforeCurrent_LeavesIndex ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (1));

      _collection.Move (2, 0);
      Assert.That (_enumerator.Index, Is.EqualTo (1));
    }

    [Test]
    public void CollectionMove_AfterCurrentToCurrent_LeavesIndex ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (1));

      _collection.Move (2, 1);
      Assert.That (_enumerator.Index, Is.EqualTo (1));
    }

    [Test]
    public void CollectionMove_BeforeCurrentToAfterCurrent_LeavesIndex ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (1));

      _collection.Move (0, 2);
      Assert.That (_enumerator.Index, Is.EqualTo (1));
    }

    [Test]
    public void CollectionMove_BeforeCurrentToCurrent_LeavesIndex ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (1));

      _collection.Move (0, 1);
      Assert.That (_enumerator.Index, Is.EqualTo (1));
    }
#endif

    [Test]
    public void CollectionClear_ResetsIndex ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Index, Is.EqualTo (2));

      _collection.Clear();
      Assert.That (_enumerator.Index, Is.EqualTo (0));
    }

    [Test]
    public void Dispose_ClearsEventListeners ()
    {
      _enumerator.Dispose();

      Assert.That (PrivateInvoke.GetNonPublicField (_collection, "CollectionChanged"), Is.Null);
    }

    [Test]
    public void IntegrationTest_ValueInserted_AfterCurrent ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.Insert (2, 100);

      Assert.That (_enumerator.Current, Is.EqualTo (1));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (100));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));
    }

    [Test]
    public void IntegrationTest_ValueInserted_BeforeCurrent ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.Insert (0, 100);

      Assert.That (_enumerator.Current, Is.EqualTo (1));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));

      _enumerator.Reset();
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (100));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (0));
    }

    [Test]
    public void IntegrationTest_ValueInserted_AtCurrent ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.Insert (1, 100);

      Assert.That (_enumerator.Current, Is.EqualTo (1));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));

      _enumerator.Reset();
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (0));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (100));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (1));
    }

    [Test]
    public void IntegrationTest_ValueRemoved_AfterCurrent ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.RemoveAt (2);

      Assert.That (_enumerator.Current, Is.EqualTo (1));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (3));
    }

    [Test]
    public void IntegrationTest_ValueRemoved_BeforeCurrent ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.RemoveAt (0);

      Assert.That (_enumerator.Current, Is.EqualTo (1));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));

      _enumerator.Reset();
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (1));
    }

    [Test]
    public void IntegrationTest_ValueRemoved_AtCurrent ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.RemoveAt (1);

      Assert.That (_enumerator.Current, Is.EqualTo (0));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (3));
    }

    [Test]
    public void IntegrationTest_ValuesCleared ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.Clear();

      Assert.That (_enumerator.MoveNext(), Is.False);
    }

    [Test]
    public void IntegrationTest_ValueSet_AtCurrent ()
    {
      _enumerator.MoveNext();
      _enumerator.MoveNext();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection[1] = 100;

      Assert.That (_enumerator.Current, Is.EqualTo (100));
      Assert.That (_enumerator.MoveNext(), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));
    }
  }
}