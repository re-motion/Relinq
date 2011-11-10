// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.Core.TestUtilities;

namespace Remotion.Linq.UnitTests.Linq.Core.TestQueryGenerators
{
  internal static class MixedTestQueryGenerator
  {
    public static IQueryable<Cook> CreateMultiFromWhereQuery (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      return from s1 in source1 from s2 in source2 where s1.Name == "Garcia" select s1;
    }


    public static IQueryable<Cook> CreateMultiFromWhereOrderByQuery (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      return from s1 in source1 from s2 in source2 where s1.Name == "Garcia" orderby s1.FirstName ascending, s2.Name descending select s1;
    }


    public static IQueryable<Cook> CreateReverseFromWhereQuery (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      return from s1 in source1 where s1.Name == "Garcia" from s2 in source2 select s1;
    }

    public static IQueryable<string> CreateReverseFromWhereQueryWithProjection (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      return from s1 in source1 where s1.Name == "Garcia" from s2 in source2 select s2.Name;
    }

    public static IQueryable<Cook> CreateThreeFromWhereQuery (IQueryable<Cook> source1, IQueryable<Cook> source2, IQueryable<Cook> source3)
    {
      return from s1 in source1 from s2 in source2 where s1.FirstName == "Hugo" from s3 in source3 select s1;
    }

    public static IQueryable<Cook> CreateWhereFromWhereQuery (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      return from s1 in source1 where s1.FirstName == "Hugo" from s2 in source2 where s1.Name == "Garcia" select s1;
    }

    public static IQueryable<Tuple<string, string, int>> CreateMultiFromQueryWithProjection (IQueryable<Cook> source1, IQueryable<Cook> source2, IQueryable<Cook> source3)
    {
      return from s1 in source1 from s2 in source2 from s3 in source3 select Tuple.Create (s1.FirstName, s2.Name, s3.ID);
    }

    public static IQueryable<Cook> CreateOrderByWithWhereCondition (IQueryable<Cook> source)
    {
      return from s in source where s.FirstName == "Garcia" orderby s.FirstName select s;
    }

    public static IQueryable<Cook> CreateOrderByWithWhereConditionAndMultiFrom (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      return from s1 in source1 where s1.FirstName == "Garcia" orderby s1.FirstName from s2 in source2 where s2.Name == "Garcia" orderby s2.FirstName, s2.Name orderby s2.FirstName select s2;
    }

    public static MethodCallExpression CreateMultiFromWhere_WhereExpression (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      IQueryable<Cook> query = CreateMultiFromWhereQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateReverseFromWhere_WhereExpression (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      IQueryable<Cook> query = CreateReverseFromWhereQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateReverseFromWhereWithProjection_SelectExpression (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      IQueryable<string> query = CreateReverseFromWhereQueryWithProjection (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateWhereFromWhere_WhereExpression (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      IQueryable<Cook> query = CreateWhereFromWhereQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateThreeFromWhereQuery_SelectManyExpression (IQueryable<Cook> source1, IQueryable<Cook> source2, IQueryable<Cook> source3)
    {
      IQueryable<Cook> query = CreateThreeFromWhereQuery (source1, source2, source3);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateOrderByQueryWithWhere_OrderByExpression (IQueryable<Cook> source)
    {
      IQueryable<Cook> query = CreateOrderByWithWhereCondition (source);
      return (MethodCallExpression) query.Expression;
    }
 }
}
