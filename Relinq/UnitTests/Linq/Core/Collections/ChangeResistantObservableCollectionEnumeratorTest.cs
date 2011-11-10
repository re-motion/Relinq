// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestUtilities;
using Remotion.Linq.Collections;

namespace Remotion.Linq.UnitTests.Linq.Core.Collections
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
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (0));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Current_WithoutMoveNext ()
    {
      Dev.Null = _enumerator.Current;
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Current_AfterLastMoveNext ()
    {
      while (_enumerator.MoveNext ())
      {
      }

      Dev.Null = _enumerator.Current;
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Current_AfterReset ()
    {
      _enumerator.MoveNext ();
      _enumerator.Reset ();
      Dev.Null = _enumerator.Current;
    }

    [Test]
    [ExpectedException (typeof (ObjectDisposedException))]
    public void Current_AfterDispose ()
    {
      _enumerator.Dispose ();
      Dev.Null = _enumerator.Current;
    }

    [Test]
    [ExpectedException (typeof (ObjectDisposedException))]
    public void Index_AfterDispose ()
    {
      _enumerator.Dispose ();
      Dev.Null = _enumerator.Index;
    }

    [Test]
    public void MoveNext_IncrementsPosition ()
    {
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (0));
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (1));
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (2));
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (3));
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (4));
    }

    [Test]
    public void MoveNext_ReturnValue ()
    {
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.MoveNext (), Is.False);
      Assert.That (_enumerator.MoveNext (), Is.False);
    }

    [Test]
    [ExpectedException (typeof (ObjectDisposedException))]
    public void MoveNext_AfterDispose ()
    {
      _enumerator.Dispose ();
      _enumerator.MoveNext();
    }

    [Test]
    public void Reset ()
    {
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (0));
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _enumerator.Reset ();
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (0));
    }

    [Test]
    [ExpectedException (typeof (ObjectDisposedException))]
    public void Reset_AfterDispose ()
    {
      _enumerator.Dispose ();
      _enumerator.Reset();
    }

    [Test]
    public void CollectionInsert_AfterCurrent_LeavesIndex ()
    {
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Index, Is.EqualTo (0));

      _collection.Insert (1, 100);
      Assert.That (_enumerator.Index, Is.EqualTo (0));
    }

    [Test]
    public void CollectionInsert_AtCurrent_IncrementsIndex ()
    {
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Index, Is.EqualTo (0));

      _collection.Insert (0, 100);
      Assert.That (_enumerator.Index, Is.EqualTo (1));
    }

    [Test]
    public void CollectionInsert_BeforeCurrent_IncrementsIndex ()
    {
      _enumerator.MoveNext ();
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Index, Is.EqualTo (1));

      _collection.Insert (0, 100);
      Assert.That (_enumerator.Index, Is.EqualTo (2));
    }

    [Test]
    public void CollectionRemove_AfterCurrent_LeavesIndex ()
    {
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Index, Is.EqualTo (0));

      _collection.RemoveAt (1);
      Assert.That (_enumerator.Index, Is.EqualTo (0));
    }

    [Test]
    public void CollectionRemove_AtCurrent_DecrementsIndex ()
    {
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Index, Is.EqualTo (0));

      _collection.RemoveAt (0);
      Assert.That (_enumerator.Index, Is.EqualTo (-1));
    }

    [Test]
    public void CollectionRemove_BeforeCurrent_DecrementsIndex ()
    {
      _enumerator.MoveNext ();
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Index, Is.EqualTo (1));

      _collection.RemoveAt (0);
      Assert.That (_enumerator.Index, Is.EqualTo (0));
    }

    [Test]
    public void Dispose_ClearsEventListeners ()
    {
      _enumerator.Dispose ();

      Assert.That (PrivateInvoke.GetNonPublicField (_collection, "ItemInserted"), Is.Null);
      Assert.That (PrivateInvoke.GetNonPublicField (_collection, "ItemRemoved"), Is.Null);
    }

    [Test]
    public void IntegrationTest_ValueInserted_AfterCurrent ()
    {
      _enumerator.MoveNext ();
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.Insert (2, 100);

      Assert.That (_enumerator.Current, Is.EqualTo (1));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (100));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));
    }

    [Test]
    public void IntegrationTest_ValueInserted_BeforeCurrent ()
    {
      _enumerator.MoveNext ();
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.Insert (0, 100);

      Assert.That (_enumerator.Current, Is.EqualTo (1));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));

      _enumerator.Reset ();
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (100));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (0));
    }

    [Test]
    public void IntegrationTest_ValueInserted_AtCurrent ()
    {
      _enumerator.MoveNext ();
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.Insert (1, 100);

      Assert.That (_enumerator.Current, Is.EqualTo (1));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));

      _enumerator.Reset ();
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (0));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (100));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (1));
    }

    [Test]
    public void IntegrationTest_ValueRemoved_AfterCurrent ()
    {
      _enumerator.MoveNext ();
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.RemoveAt (2);

      Assert.That (_enumerator.Current, Is.EqualTo (1));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (3));
    }

    [Test]
    public void IntegrationTest_ValueRemoved_BeforeCurrent ()
    {
      _enumerator.MoveNext ();
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.RemoveAt (0);

      Assert.That (_enumerator.Current, Is.EqualTo (1));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));

      _enumerator.Reset ();
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (1));
    }

    [Test]
    public void IntegrationTest_ValueRemoved_AtCurrent ()
    {
      _enumerator.MoveNext ();
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.RemoveAt (1);

      Assert.That (_enumerator.Current, Is.EqualTo (0));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (3));
    }

    [Test]
    public void IntegrationTest_ValuesCleared ()
    {
      _enumerator.MoveNext ();
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection.Clear();

      Assert.That (_enumerator.MoveNext (), Is.False);
    }

    [Test]
    public void IntegrationTest_ValueSet_AtCurrent ()
    {
      _enumerator.MoveNext ();
      _enumerator.MoveNext ();
      Assert.That (_enumerator.Current, Is.EqualTo (1));

      _collection[1] = 100;

      Assert.That (_enumerator.Current, Is.EqualTo (100));
      Assert.That (_enumerator.MoveNext (), Is.True);
      Assert.That (_enumerator.Current, Is.EqualTo (2));
    }
  }
}