// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.TestQueryGenerators
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
