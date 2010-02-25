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
    public static IQueryable<Student> CreateSimpleExplicitJoin (IQueryable<Student_Detail> source1, IQueryable<Student> source2)
    {
      return from s1 in source2 join s2 in source1 on s1.ID equals s2.StudentID select s1;
    }

    public static IQueryable<Student_Detail> CreateSimpleImplicitOrderByJoin (IQueryable<Student_Detail> source)
    {
      return from sd in source orderby sd.Student.FirstName select sd;
    }

    public static IQueryable<Student_Detail_Detail> CreateDoubleImplicitOrderByJoin (IQueryable<Student_Detail_Detail> source)
    {
      return from sdd in source orderby sdd.Student_Detail.Student.FirstName select sdd;
    }

    public static IQueryable<Student_Detail_Detail> CreateImplicitOrderByJoinWithMultipleJoins (IQueryable<Student_Detail_Detail> source)
    {
      return from sdd in source orderby sdd.Student_Detail.Student.FirstName, sdd.IndustrialSector.ID select sdd;
    }

    public static IQueryable<Student_Detail_Detail> CreateImplicitOrderByJoinCheckingCorrectNumberOfEntries (IQueryable<Student_Detail_Detail> source)
    {
      return from sdd in source orderby sdd.Student_Detail.Student.FirstName, sdd.Student_Detail.Student.Name select sdd;
    }

    public static IQueryable<Student_Detail_Detail> CreateImplicitOrderByJoinWithDifferentLevels (IQueryable<Student_Detail_Detail> source)
    {
      return from sdd in source orderby sdd.Student_Detail.Student.FirstName, sdd.Student_Detail.IndustrialSector.ID select sdd;
    }

    public static IQueryable<string> CreateSimpleImplicitSelectJoin (IQueryable<Student_Detail> source)
    {
      return from sd in source select sd.Student.FirstName;
    }

    public static IQueryable<Tuple<string, int>> CreateComplexImplicitSelectJoin (IQueryable<Student_Detail_Detail> source)
    {
      return from sdd in source select new Tuple<string, int> (sdd.Student_Detail.Student.FirstName, sdd.IndustrialSector.ID);
    }

    public static IQueryable<Student_Detail> CreateSimpleImplicitWhereJoin (IQueryable<Student_Detail> source)
    {
      return from sd in source where sd.Student.FirstName == "Garcia" select sd;
    }

    public static IQueryable<Student_Detail_Detail> CreateImplicitOrderByJoinWithMultipleKeys
    (IQueryable<Student_Detail_Detail> source1, IQueryable<Student_Detail_Detail> source2)
    {
      return from sdd1 in source1
             from sdd2 in source2
             orderby sdd1.Student_Detail.Student.FirstName
             orderby sdd2.Student_Detail.Student.FirstName
             select sdd1;
    }

    public static IQueryable<Student_Detail_Detail> CreateImplicitOrderByJoinWithJoinReuse
        (IQueryable<Student_Detail_Detail> source1, IQueryable<Student_Detail_Detail> source2)
    {
      return from sdd1 in source1
             from sdd2 in source2
             orderby sdd1.Student_Detail.Student.FirstName
             orderby sdd2.Student_Detail.Student.FirstName
             orderby sdd1.Student_Detail.Student.FirstName
             select sdd1;
    }

    public static IQueryable<Student_Detail_Detail> CreateImplicitOrderByJoinWithJoinPartReuse (IQueryable<Student_Detail_Detail> source)
    {
      return from sdd in source
             orderby sdd.Student_Detail.Student.FirstName
             orderby sdd.Student_Detail.ID
             select sdd;
    }
  }
}
