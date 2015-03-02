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
using Remotion.Utilities;

// ReSharper disable once CheckNamespace
namespace System
{
  public class Tuple<T1, T2, T3> : IEquatable<Tuple<T1, T2, T3>>
  {
    private readonly T1 _item1;
    private readonly T2 _item2;
    private readonly T3 _item3;

    public Tuple (T1 item1, T2 item2, T3 item3)
    {
      _item1 = item1;
      _item2 = item2;
      _item3 = item3;
    }

    public T1 Item1
    {
      get { return _item1; }
    }

    public T2 Item2
    {
      get { return _item2; }
    }

    public T3 Item3
    {
      get { return _item3; }
    }

    public bool Equals (Tuple<T1, T2, T3> other)
    {
      return EqualityUtility.NotNullAndSameType (this, other)
             && EqualityUtility.Equals (_item1, other._item1)
             && EqualityUtility.Equals (_item2, other._item2)
             && EqualityUtility.Equals (_item3, other._item3);
    }

    public override bool Equals (object obj)
    {
      return EqualityUtility.EqualsEquatable (this, obj);
    }

    public override int GetHashCode ()
    {
      return EqualityUtility.GetRotatedHashCode (_item1, _item2, _item3);
    }

    public override string ToString ()
    {
      return string.Format ("<{0}, {1}, {2}>", _item1, _item2, _item3);
    }
  }
}
