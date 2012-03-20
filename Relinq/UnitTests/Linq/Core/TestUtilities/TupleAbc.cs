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
#if NET_3_5

// Simulate System Tuple types from .NET 4 in .NET 3.5

namespace System
{
  /// <summary>
  /// Only simulates structure of .NET 4.0 Tuple, not behavior.
  /// </summary>
  internal class Tuple<T1, T2, T3>
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

    public override string ToString ()
    {
      return string.Format ("<{0}, {1}, {2}>", _item1, _item2, _item3);
    }

    public override bool Equals (object obj)
    {
      throw new NotSupportedException ("Tuple<,,> only simulates structure of tuple, not behavior.");
    }

    public override int GetHashCode ()
    {
      throw new NotSupportedException ("Tuple<,,> only simulates structure of tuple, not behavior.");
    }
  }
}
#endif
