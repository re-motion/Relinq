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
  public static class WhereTestQueryGenerator
  {
    public static IQueryable<Cook> CreateSimpleWhereQuery (IQueryable<Cook> source)
    {
      return from s in source where s.Name == "Garcia" select s;
    }

    public static IQueryable<Cook> CreateWhereQueryWithEvaluatableSubExpression (IQueryable<Cook> source)
    {
      string cia = "cia";
      return from s in source where s.Name == ("Gar" + cia) select s;
    }

    public static IQueryable<Cook> CreateMultiWhereQuery (IQueryable<Cook> source)
    {
      return from s in source where s.Name == "Garcia" where s.FirstName == "Hugo" where s.ID > 100 select s;
    }

    public static IQueryable<string> CreateSelectWhereQuery (IQueryable<Cook> source)
    {
      return from s in source where s.Name == "Garcia" select s.FirstName;
    }

    public static IQueryable<Cook> CreateWhereQueryWithDifferentComparisons (IQueryable<Cook> source)
    {
      return from s in source where s.FirstName != "Garcia" && s.ID > 5 && s.ID >= 6 && s.ID < 7 && s.ID <= 6 && s.ID == 6 select s;
    }

    public static IQueryable<Cook> CreateWhereQueryWithOrAndNot (IQueryable<Cook> source)
    {
      return from s in source where (!(s.FirstName == "Garcia") || s.FirstName == "Garcia") && s.FirstName == "Garcia" select s;
    }

    public static IQueryable<Cook> CreateWhereQueryWithStartsWith (IQueryable<Cook> source)
    {
      return from s in source where s.FirstName.StartsWith("Garcia") select s;
    }

    public static IQueryable<Cook> CreateWhereQueryWithEndsWith (IQueryable<Cook> source)
    {
      return from s in source where s.FirstName.EndsWith("Garcia") select s;
    }

    public static IQueryable<Cook> CreateWhereQueryNullChecks (IQueryable<Cook> source)
    {
      return from s in source where s.FirstName == null || null != s.Name select s;
    }

    public static IQueryable<Cook> CreateWhereQueryBooleanConstantTrue (IQueryable<Cook> source)
    {
      return from s in source where true select s;
    }

    public static IQueryable<Cook> CreateWhereQueryBooleanConstantFalse (IQueryable<Cook> source)
    {
      return from s in source where false select s;
    }

    public static MethodCallExpression CreateSimpleWhereQuery_WhereExpression (IQueryable<Cook> source)
    {
      IQueryable<Cook> query = CreateSimpleWhereQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateWhereQueryWithEvaluatableSubExpression_WhereExpression (IQueryable<Cook> source)
    {
      IQueryable<Cook> query = CreateWhereQueryWithEvaluatableSubExpression (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateMultiWhereQuery_WhereExpression (IQueryable<Cook> source)
    {
      IQueryable<Cook> query = CreateMultiWhereQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static IQueryable<Kitchen> CreateRelationMemberWhereQuery (IQueryable<Kitchen> source)
    {
      return from sd in source where sd.Restaurant != null select sd;
    }

    public static IQueryable<Restaurant> CreateRelationMemberVirtualSideWhereQuery (IQueryable<Restaurant> source)
    {
      return from restaurant in source where restaurant.SubKitchen != null select restaurant;
    }

    public static MethodCallExpression CreateSelectWhereQuery_SelectExpression (IQueryable<Cook> source)
    {
      IQueryable<string> query = CreateSelectWhereQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static IQueryable<Kitchen> CreateWhereQueryWithRelatedPrimaryKey_VirtualColumn (IQueryable<Kitchen> source)
    {
      return from sd in source where sd.Cook.ID == 5 select sd;
    }

    public static IQueryable<Kitchen> CreateWhereQueryWithRelatedPrimaryKey_RealColumn (IQueryable<Kitchen> source)
    {
      return from sd in source where sd.Restaurant.ID == 5 select sd;
    }
  }
}
