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
using System.Linq;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.TestQueryGenerators
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
      return from sdd in source orderby sdd.MainKitchen.Cook.FirstName, sdd.MainRestaurant.ID select sdd;
    }

    public static IQueryable<Company> CreateImplicitOrderByJoinCheckingCorrectNumberOfEntries (IQueryable<Company> source)
    {
      return from sdd in source orderby sdd.MainKitchen.Cook.FirstName, sdd.MainKitchen.Cook.Name select sdd;
    }

    public static IQueryable<Company> CreateImplicitOrderByJoinWithDifferentLevels (IQueryable<Company> source)
    {
      return from sdd in source orderby sdd.MainKitchen.Cook.FirstName, sdd.MainKitchen.Restaurant.ID select sdd;
    }

    public static IQueryable<string> CreateSimpleImplicitSelectJoin (IQueryable<Kitchen> source)
    {
      return from sd in source select sd.Cook.FirstName;
    }

    public static IQueryable<Tuple<string, int>> CreateComplexImplicitSelectJoin (IQueryable<Company> source)
    {
      return from sdd in source select new Tuple<string, int> (sdd.MainKitchen.Cook.FirstName, sdd.MainRestaurant.ID);
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
