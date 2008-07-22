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
  public static class LetTestQueryGenerator
  {
    public static IQueryable<string> CreateSimpleLetClause (IQueryable<Student> source)
    {
      return from s in source let x = s.First + s.Last select x;
    }

    public static IQueryable<string> CreateLet_WithJoin_NoTable (IQueryable<Student_Detail> source)
    {
      return from sd in source let x = sd.Student.First select x;
    }

    public static IQueryable<Student> CreateLet_WithJoin_WithTable (IQueryable<Student_Detail> source)
    {
      return from sd in source let x = sd.Student select x;
    }

    public static IQueryable<Student> CreateLet_WithTable (IQueryable<Student> source)
    {
      return from s in source let x = s select x;
    }

    public static IQueryable<string> CreateMultiLet_WithWhere (IQueryable<Student> source)
    {
      return from s in source let x = s.First let y = s.ID where y > 1 select x;
    }


    public static MethodCallExpression CreateSimpleSelect_LetExpression (IQueryable<Student> source)
    {
      IQueryable<string> query = CreateSimpleLetClause (source);
      return (MethodCallExpression) query.Expression;
    }
  }
}
