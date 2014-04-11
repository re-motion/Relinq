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

namespace Remotion.Linq.UnitTests.Linq.Core.TestQueryGenerators
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
