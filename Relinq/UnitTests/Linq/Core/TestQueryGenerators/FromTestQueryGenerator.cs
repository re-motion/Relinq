// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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

namespace Remotion.Linq.UnitTests.Linq.Core.TestQueryGenerators
{
  public static class FromTestQueryGenerator
  {
    public static IQueryable<Cook> CreateMultiFromQuery (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      return from s1 in source1 from s2 in source2 select s1;
    }

    public static IQueryable<Cook> CreateMultiFromQuery_WithCalls (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      return from s1 in GetSource (source1) from s2 in GetSource (source2) select s1;
    }

    private static IQueryable<Cook> GetSource (IQueryable<Cook> source)
    {
      return source;
    }

    public static IQueryable<Cook> CreateThreeFromQuery (IQueryable<Cook> source1, IQueryable<Cook> source2, IQueryable<Cook> source3)
    {
      return from s1 in source1 from s2 in source2 from s3 in source3 select s1;
    }

    public static IQueryable<Cook> CreateFromQueryWithMemberQuerySource (IQueryable<Restaurant> source)
    {
      return from sector in source
             from s1 in sector.Cooks
             select s1;
    }

    public static IQueryable<Cook> CreateFromQueryWithMemberQuerySource_InMainFromClauseOfSubQuery (IQueryable<Restaurant> source)
    {
      return from sector in source
             from s1 in (from s2 in sector.Cooks select s2)
             select s1;
    }

    public static IQueryable<Cook> CreateFromQueryWithMemberQuerySourceAndOptimizableJoin (IQueryable<Kitchen> source)
    {
      return from sd in source
             from s1 in sd.Restaurant.Cooks
             select s1;
    }

    public static IQueryable<Cook> CreateFromQueryWithMemberQuerySourceAndJoin (IQueryable<Company> source)
    {
      return from sdd in source
             from s1 in sdd.MainRestaurant.Cooks
             select s1;
    }

    public static MethodCallExpression CreateMultiFromQuery_SelectManyExpression (IQueryable<Cook> source1, IQueryable<Cook> source2)
    {
      IQueryable<Cook> query = CreateMultiFromQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateThreeFromQuery_SelectManyExpression (IQueryable<Cook> source1, IQueryable<Cook> source2, IQueryable<Cook> source3)
    {
      IQueryable<Cook> query = CreateThreeFromQuery (source1, source2, source3);
      return (MethodCallExpression) query.Expression;
    }
  }
}
