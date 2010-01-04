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
using Remotion.Data.Linq.UnitTests.Utilities;

namespace Remotion.Data.Linq.UnitTests.TestQueryGenerators
{
  public static class MixedTestQueryGenerator
  {
    public static IQueryable<Student> CreateMultiFromWhereQuery (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 from s2 in source2 where s1.Last == "Garcia" select s1;
    }


    public static IQueryable<Student> CreateMultiFromWhereOrderByQuery (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 from s2 in source2 where s1.Last == "Garcia" orderby s1.First ascending, s2.Last descending select s1;
    }


    public static IQueryable<Student> CreateReverseFromWhereQuery (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 where s1.Last == "Garcia" from s2 in source2 select s1;
    }

    public static IQueryable<string> CreateReverseFromWhereQueryWithProjection (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 where s1.Last == "Garcia" from s2 in source2 select s2.Last;
    }

    public static IQueryable<Student> CreateThreeFromWhereQuery (IQueryable<Student> source1, IQueryable<Student> source2, IQueryable<Student> source3)
    {
      return from s1 in source1 from s2 in source2 where s1.First == "Hugo" from s3 in source3 select s1;
    }

    public static IQueryable<Student> CreateWhereFromWhereQuery (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 where s1.First == "Hugo" from s2 in source2 where s1.Last == "Garcia" select s1;
    }

    public static IQueryable<Tuple<string, string, int>> CreateMultiFromQueryWithProjection (IQueryable<Student> source1, IQueryable<Student> source2, IQueryable<Student> source3)
    {
      return from s1 in source1 from s2 in source2 from s3 in source3 select Tuple.NewTuple (s1.First, s2.Last, s3.ID);
    }

    public static IQueryable<Student> CreateOrderByWithWhereCondition (IQueryable<Student> source)
    {
      return from s in source where s.First == "Garcia" orderby s.First select s;
    }

    public static IQueryable<Student> CreateOrderByWithWhereConditionAndMultiFrom (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 where s1.First == "Garcia" orderby s1.First from s2 in source2 where s2.Last == "Garcia" orderby s2.First, s2.Last orderby s2.First select s2;
    }

    public static MethodCallExpression CreateMultiFromWhere_WhereExpression (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      IQueryable<Student> query = CreateMultiFromWhereQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateReverseFromWhere_WhereExpression (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      IQueryable<Student> query = CreateReverseFromWhereQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateReverseFromWhereWithProjection_SelectExpression (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      IQueryable<string> query = CreateReverseFromWhereQueryWithProjection (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateWhereFromWhere_WhereExpression (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      IQueryable<Student> query = CreateWhereFromWhereQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateThreeFromWhereQuery_SelectManyExpression (IQueryable<Student> source1, IQueryable<Student> source2, IQueryable<Student> source3)
    {
      IQueryable<Student> query = CreateThreeFromWhereQuery (source1, source2, source3);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateOrderByQueryWithWhere_OrderByExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateOrderByWithWhereCondition (source);
      return (MethodCallExpression) query.Expression;
    }
 }
}
