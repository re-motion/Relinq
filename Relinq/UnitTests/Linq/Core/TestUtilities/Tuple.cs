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
  internal static class Tuple
  {
    public static Tuple<TA, TB> Create<TA, TB> (TA a, TB b)
    {
      return new Tuple<TA, TB> (a, b);
    }

    public static Tuple<TA, TB, TC> Create<TA, TB, TC> (TA a, TB b, TC c)
    {
      return new Tuple<TA, TB, TC> (a, b, c);
    }

    public static Tuple<TA, TB, TC, TD> Create<TA, TB, TC, TD> (TA a, TB b, TC c, TD d)
    {
      return new Tuple<TA, TB, TC, TD> (a, b, c, d);
    }
  }
}
#endif
