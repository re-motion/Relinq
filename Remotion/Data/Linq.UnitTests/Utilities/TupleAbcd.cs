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
using Remotion.Utilities;

namespace Remotion.Data.Linq.UnitTests.Utilities
{
  // TODO: Doc
  [Serializable]
  public class Tuple<TA, TB, TC, TD> : IEquatable<Tuple<TA, TB, TC, TD>>
  {
    private readonly TA _a;
    private readonly TB _b;
    private readonly TC _c;
    private readonly TD _d;

    public Tuple (TA a, TB b, TC c, TD d)
    {
      _a = a;
      _b = b;
      _c = c;
      _d = d;
    }

    public TA A
    {
      get { return _a; }
    }

    public TB B
    {
      get { return _b; }
    }

    public TC C
    {
      get { return _c; }
    }

    public TD D
    {
      get { return _d; }
    }

    public bool Equals (Tuple<TA, TB, TC, TD> other)
    {
      if (other == null)
        return false;

      return EqualityUtility.Equals (_a, other._a)
             && EqualityUtility.Equals (_b, other._b)
             && EqualityUtility.Equals (_c, other._c)
             && EqualityUtility.Equals (_d, other._d);
    }

    public override bool Equals (object obj)
    {
      Tuple<TA, TB, TC, TD> other = obj as Tuple<TA, TB, TC, TD>;
      if (other == null)
        return false;
      return Equals (other);
    }

    public override int GetHashCode ()
    {
      return EqualityUtility.GetRotatedHashCode (_a, _b, _c, _d);
    }

    public override string ToString ()
    {
      return string.Format ("<{0}, {1}, {2}, {3}>", _a, _b, _c, _d);
    }
  }
}
