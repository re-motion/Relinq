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

namespace Remotion.Data.Linq.UnitTests.TestQueryGenerators
{
  public static class OrderByTestQueryGenerator
  {
    public static IQueryable<Student> CreateSimpleOrderByQuery (IQueryable<Student> source)
    {
      return from s1 in source orderby s1.First select s1;
    }

    public static IQueryable<Student_Detail> CreateRelationMemberOrderByQuery (IQueryable<Student_Detail> source)
    {
      return from sd in source orderby sd.Student select sd;
    }

    public static IQueryable<Student> CreateOrderByNonDBPropertyQuery (IQueryable<Student> source)
    {
      return from s1 in source orderby s1.NonDBProperty select s1;
    }

    public static IQueryable<Student> CreateTwoOrderByQuery (IQueryable<Student> source)
    {
      return from s1 in source orderby s1.First orderby s1.Last descending select s1;
    }

    public static IQueryable<Student> CreateThreeOrderByQuery (IQueryable<Student> source)
    {
      return from s1 in source orderby s1.First, s1.Last orderby s1.Last descending select s1;
    }

    public static IQueryable<Student> CreateOrderByQueryWithOrderByAndThenBy (IQueryable<Student> source)
    {
      return from s in source orderby s.First, s.Last descending, s.Scores select s;
    }

    public static IQueryable<Student> CreateOrderByQueryWithMultipleOrderBys (IQueryable<Student> source)
    {
      return from s in source orderby s.First, s.Last descending, s.Scores orderby s.Last select s;
    }

    public static MethodCallExpression CreateOrderByQueryWithOrderByAndThenBy_OrderByExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateOrderByQueryWithOrderByAndThenBy (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateOrderByQueryWithMultipleOrderBys_OrderByExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateOrderByQueryWithMultipleOrderBys (source);
      return (MethodCallExpression) query.Expression;
    }

  }
}
