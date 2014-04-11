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
  public static class OrderByTestQueryGenerator
  {
    public static IQueryable<Cook> CreateSimpleOrderByQuery (IQueryable<Cook> source)
    {
      return from s1 in source orderby s1.FirstName select s1;
    }

    public static IQueryable<Kitchen> CreateRelationMemberOrderByQuery (IQueryable<Kitchen> source)
    {
      return from sd in source orderby sd.Cook select sd;
    }

    public static IQueryable<Cook> CreateOrderByNonDBPropertyQuery (IQueryable<Cook> source)
    {
      return from s1 in source orderby s1.NonDBStringProperty select s1;
    }

    public static IQueryable<Cook> CreateTwoOrderByQuery (IQueryable<Cook> source)
    {
      return from s1 in source orderby s1.FirstName orderby s1.Name descending select s1;
    }

    public static IQueryable<Cook> CreateThreeOrderByQuery (IQueryable<Cook> source)
    {
      return from s1 in source orderby s1.FirstName, s1.Name orderby s1.Name descending select s1;
    }

    public static IQueryable<Cook> CreateOrderByQueryWithOrderByAndThenBy (IQueryable<Cook> source)
    {
      return from s in source orderby s.FirstName, s.Name descending, s.Holidays select s;
    }

    public static IQueryable<Cook> CreateOrderByQueryWithMultipleOrderBys (IQueryable<Cook> source)
    {
      return from s in source orderby s.FirstName, s.Name descending, s.Holidays orderby s.Name select s;
    }

    public static MethodCallExpression CreateOrderByQueryWithOrderByAndThenBy_OrderByExpression (IQueryable<Cook> source)
    {
      IQueryable<Cook> query = CreateOrderByQueryWithOrderByAndThenBy (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateOrderByQueryWithMultipleOrderBys_OrderByExpression (IQueryable<Cook> source)
    {
      IQueryable<Cook> query = CreateOrderByQueryWithMultipleOrderBys (source);
      return (MethodCallExpression) query.Expression;
    }

  }
}
