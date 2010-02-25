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
using System.Linq;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.UnitTests.TestUtilities;

namespace Remotion.Data.Linq.UnitTests.TestQueryGenerators
{
  internal static class JoinTestQueryGenerator
  {
    public static IQueryable<Cook> CreateSimpleExplicitJoin (IQueryable<Kitchen> source1, IQueryable<Cook> source2)
    {
      return from s1 in source2 join s2 in source1 on s1.ID equals s2.RoomNumber select s1;
    }

    public static IQueryable<Kitchen> CreateSimpleImplicitOrderByJoin (IQueryable<Kitchen> source)
    {
      return from sd in source orderby sd.Cook.FirstName select sd;
    }

    public static IQueryable<Restaurant> CreateDoubleImplicitOrderByJoin (IQueryable<Restaurant> source)
    {
      return from sdd in source orderby sdd.Kitchen.Cook.FirstName select sdd;
    }

    public static IQueryable<Restaurant> CreateImplicitOrderByJoinWithMultipleJoins (IQueryable<Restaurant> source)
    {
      return from sdd in source orderby sdd.Kitchen.Cook.FirstName, sdd.IndustrialSector.ID select sdd;
    }

    public static IQueryable<Restaurant> CreateImplicitOrderByJoinCheckingCorrectNumberOfEntries (IQueryable<Restaurant> source)
    {
      return from sdd in source orderby sdd.Kitchen.Cook.FirstName, sdd.Kitchen.Cook.Name select sdd;
    }

    public static IQueryable<Restaurant> CreateImplicitOrderByJoinWithDifferentLevels (IQueryable<Restaurant> source)
    {
      return from sdd in source orderby sdd.Kitchen.Cook.FirstName, sdd.Kitchen.IndustrialSector.ID select sdd;
    }

    public static IQueryable<string> CreateSimpleImplicitSelectJoin (IQueryable<Kitchen> source)
    {
      return from sd in source select sd.Cook.FirstName;
    }

    public static IQueryable<Tuple<string, int>> CreateComplexImplicitSelectJoin (IQueryable<Restaurant> source)
    {
      return from sdd in source select new Tuple<string, int> (sdd.Kitchen.Cook.FirstName, sdd.IndustrialSector.ID);
    }

    public static IQueryable<Kitchen> CreateSimpleImplicitWhereJoin (IQueryable<Kitchen> source)
    {
      return from sd in source where sd.Cook.FirstName == "Garcia" select sd;
    }

    public static IQueryable<Restaurant> CreateImplicitOrderByJoinWithMultipleKeys
    (IQueryable<Restaurant> source1, IQueryable<Restaurant> source2)
    {
      return from sdd1 in source1
             from sdd2 in source2
             orderby sdd1.Kitchen.Cook.FirstName
             orderby sdd2.Kitchen.Cook.FirstName
             select sdd1;
    }

    public static IQueryable<Restaurant> CreateImplicitOrderByJoinWithJoinReuse
        (IQueryable<Restaurant> source1, IQueryable<Restaurant> source2)
    {
      return from sdd1 in source1
             from sdd2 in source2
             orderby sdd1.Kitchen.Cook.FirstName
             orderby sdd2.Kitchen.Cook.FirstName
             orderby sdd1.Kitchen.Cook.FirstName
             select sdd1;
    }

    public static IQueryable<Restaurant> CreateImplicitOrderByJoinWithJoinPartReuse (IQueryable<Restaurant> source)
    {
      return from sdd in source
             orderby sdd.Kitchen.Cook.FirstName
             orderby sdd.Kitchen.ID
             select sdd;
    }
  }
}
