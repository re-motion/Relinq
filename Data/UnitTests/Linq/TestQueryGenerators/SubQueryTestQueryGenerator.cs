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

namespace Remotion.Data.UnitTests.Linq.TestQueryGenerators
{
  public class SubQueryTestQueryGenerator
  {
    public static IQueryable<Student> CreateSimpleSubQueryInAdditionalFromClause(IQueryable<Student> source)
    {
      return from s in source from s2 in (from s3 in source select s3) select s;
    }

    public static IQueryable<Student> CreateComplexSubQueryInAdditionalFromClause (IQueryable<Student> source)
    {
      return from s in source from s2 in (from s3 in source where s3.ID == s.ID && s3.ID > 3 select s3) select s2;
    }

    public static IQueryable<Student> CreateSimpleSubQueryInWhereClause (IQueryable<Student> source)
    {
      return from s in source where (from s2 in source select s2).Contains (s) select s;
    }

    public static IQueryable<Student> CreateSubQueryWithConstantInWhereClause (IQueryable<Student> source)
    {
      Student student = new Student {ID = 5};
      return from s in source where (from s2 in source select s2).Contains (student) select s;
    }

    

    public static IQueryable<Student> CreateSubQuerySelectingColumnsWithConstantInWhereClause (IQueryable<Student> source)
    {
      return from s in source where (from s2 in source select s2.First).Contains ("Hugo") select s;
    }
  }
}
