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

namespace Remotion.Data.Linq.UnitTests.TestUtilities
{
  [Serializable]
  public struct Tuple<TA, TB> : IEquatable<Tuple<TA, TB>>
  {
    private readonly TA _a;
    private readonly TB _b;

    public Tuple (TA a, TB b)
    {
      _a = a;
      _b = b;
    }

    public TA A
    {
      get { return _a; }
    }

    public TB B
    {
      get { return _b; }
    }

    public bool Equals (Tuple<TA, TB> other)
    {
      return Equals ((object) other);
    }

    public override string ToString ()
    {
      return string.Format ("<{0}, {1}>", _a, _b);
    }
  }
}
