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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Remotion.Data.Linq.UnitTests.TestDomain
{
  public class Cook
  {
    public string FirstName { get; set; }
    public string Name { get; set; }
    public int ID { get; set; }
    public ArrayList Courses { get; set; }
    public int[] EctsCredits { get; set; }
    public List<int> Scores { get; set; }
    public IQueryable<Cook> Assistants { get; set; }
    public string NonDBStringProperty { get; set; }
    public bool NonDBBoolProperty { get; set; }
    public bool IsExchangeStudent { get; set; }
    public bool HasDegree { get; set; }
    public Cook BuddyCook { get; set; }
  }
}
