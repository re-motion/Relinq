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

    public static IQueryable<Company> CreateDoubleImplicitOrderByJoin (IQueryable<Company> source)
    {
      return from sdd in source orderby sdd.MainKitchen.Cook.FirstName select sdd;
    }

    public static IQueryable<Company> CreateImplicitOrderByJoinWithMultipleJoins (IQueryable<Company> source)
    {
      return from sdd in source orderby sdd.MainKitchen.Cook.FirstName, sdd.IndustrialSector.ID select sdd;
    }

    public static IQueryable<Company> CreateImplicitOrderByJoinCheckingCorrectNumberOfEntries (IQueryable<Company> source)
    {
      return from sdd in source orderby sdd.MainKitchen.Cook.FirstName, sdd.MainKitchen.Cook.Name select sdd;
    }

    public static IQueryable<Company> CreateImplicitOrderByJoinWithDifferentLevels (IQueryable<Company> source)
    {
      return from sdd in source orderby sdd.MainKitchen.Cook.FirstName, sdd.MainKitchen.IndustrialSector.ID select sdd;
    }

    public static IQueryable<string> CreateSimpleImplicitSelectJoin (IQueryable<Kitchen> source)
    {
      return from sd in source select sd.Cook.FirstName;
    }

    public static IQueryable<Tuple<string, int>> CreateComplexImplicitSelectJoin (IQueryable<Company> source)
    {
      return from sdd in source select new Tuple<string, int> (sdd.MainKitchen.Cook.FirstName, sdd.IndustrialSector.ID);
    }

    public static IQueryable<Kitchen> CreateSimpleImplicitWhereJoin (IQueryable<Kitchen> source)
    {
      return from sd in source where sd.Cook.FirstName == "Garcia" select sd;
    }

    public static IQueryable<Company> CreateImplicitOrderByJoinWithMultipleKeys
    (IQueryable<Company> source1, IQueryable<Company> source2)
    {
      return from sdd1 in source1
             from sdd2 in source2
             orderby sdd1.MainKitchen.Cook.FirstName
             orderby sdd2.MainKitchen.Cook.FirstName
             select sdd1;
    }

    public static IQueryable<Company> CreateImplicitOrderByJoinWithJoinReuse
        (IQueryable<Company> source1, IQueryable<Company> source2)
    {
      return from sdd1 in source1
             from sdd2 in source2
             orderby sdd1.MainKitchen.Cook.FirstName
             orderby sdd2.MainKitchen.Cook.FirstName
             orderby sdd1.MainKitchen.Cook.FirstName
             select sdd1;
    }

    public static IQueryable<Company> CreateImplicitOrderByJoinWithJoinPartReuse (IQueryable<Company> source)
    {
      return from sdd in source
             orderby sdd.MainKitchen.Cook.FirstName
             orderby sdd.MainKitchen.ID
             select sdd;
    }
  }
}
