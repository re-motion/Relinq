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
  public static class DistinctTestQueryGenerator
  {
    public static IQueryable<string> CreateSimpleDistinctQuery (IQueryable<Student> source)
    {
      return (from s in source select s.First).Distinct ();
    }

    public static IQueryable<Student> CreateDisinctWithWhereQueryWithoutProjection (IQueryable<Student> source)
    {
      return (from s in source where s.First == "Garcia" select s).Distinct ();
    }

    public static IQueryable<string> CreateDisinctWithWhereQuery (IQueryable<Student> source)
    {
      return (from s in source where s.First == "Garcia" select s.First).Distinct ();
    }

    public static MethodCallExpression CreateSimpleDistinctQuery_MethodCallExpression (IQueryable<Student> source)
    {
      IQueryable<string> query = CreateSimpleDistinctQuery (source);
      MethodCallExpression newQuery = (MethodCallExpression) ((MethodCallExpression) query.Expression).Arguments[0];
      return newQuery;
    }
  }
}
