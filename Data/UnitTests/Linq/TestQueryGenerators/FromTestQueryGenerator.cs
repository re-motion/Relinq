/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Linq;
using System.Linq.Expressions;

namespace Remotion.Data.UnitTests.Linq.TestQueryGenerators
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
