// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Collections.Generic;

namespace Remotion.Data.UnitTests.Linq.Parsing
{
  public class AnonymousType<TA, TB>
  {
    public AnonymousType ()
    {
    }

    public AnonymousType (TA a, TB b)
    {
      this.a = a;
      this.b = b;
    }

    public TA a { get; set; }
    public TB b { get; set; }
    public List<int> List { get; set; }
  }

  public class AnonymousType
  {
    public AnonymousType ()
    {
    }

    public AnonymousType (int a, int b)
    {
      this.a = a;
      this.b = b;
    }

    public int a { get; set; }
    public int b { get; set; }
    public List<int> List { get; set; }
  }
}