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
using System.Linq.Expressions;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.TestQueryGenerators
{
  public static class FromTestQueryGenerator
  {
    public static IQueryable<Student> CreateMultiFromQuery (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 from s2 in source2 select s1;
    }

    public static IQueryable<Student> CreateMultiFromQuery_WithCalls (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in GetSource (source1) from s2 in GetSource (source2) select s1;
    }

    private static IQueryable<Student> GetSource (IQueryable<Student> source)
    {
      return source;
    }

    public static IQueryable<Student> CreateThreeFromQuery (IQueryable<Student> source1, IQueryable<Student> source2, IQueryable<Student> source3)
    {
      return from s1 in source1 from s2 in source2 from s3 in source3 select s1;
    }

    public static IQueryable<Student> CreateFromQueryWithMemberQuerySource (IQueryable<IndustrialSector> source)
    {
      return from sector in source
             from s1 in sector.Students
             select s1;
    }

    public static IQueryable<Student> CreateFromQueryWithMemberQuerySource_InMainFromClauseOfSubQuery (IQueryable<IndustrialSector> source)
    {
      return from sector in source
             from s1 in (from s2 in sector.Students select s2)
             select s1;
    }

    public static IQueryable<Student> CreateFromQueryWithMemberQuerySourceAndOptimizableJoin (IQueryable<Student_Detail> source)
    {
      return from sd in source
             from s1 in sd.IndustrialSector.Students
             select s1;
    }

    public static IQueryable<Student> CreateFromQueryWithMemberQuerySourceAndJoin (IQueryable<Student_Detail_Detail> source)
    {
      return from sdd in source
             from s1 in sdd.IndustrialSector.Students
             select s1;
    }

    public static MethodCallExpression CreateMultiFromQuery_SelectManyExpression (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      IQueryable<Student> query = CreateMultiFromQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateThreeFromQuery_SelectManyExpression (IQueryable<Student> source1, IQueryable<Student> source2, IQueryable<Student> source3)
    {
      IQueryable<Student> query = CreateThreeFromQuery (source1, source2, source3);
      return (MethodCallExpression) query.Expression;
    }
  }
}
