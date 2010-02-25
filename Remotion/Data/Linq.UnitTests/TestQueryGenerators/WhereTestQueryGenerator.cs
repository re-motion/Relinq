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

namespace Remotion.Data.Linq.UnitTests.TestQueryGenerators
{
  public static class WhereTestQueryGenerator
  {
    public static IQueryable<Chef> CreateSimpleWhereQuery (IQueryable<Chef> source)
    {
      return from s in source where s.Name == "Garcia" select s;
    }

    public static IQueryable<Chef> CreateWhereQueryWithEvaluatableSubExpression (IQueryable<Chef> source)
    {
      string cia = "cia";
      return from s in source where s.Name == ("Gar" + cia) select s;
    }

    public static IQueryable<Chef> CreateMultiWhereQuery (IQueryable<Chef> source)
    {
      return from s in source where s.Name == "Garcia" where s.FirstName == "Hugo" where s.ID > 100 select s;
    }

    public static IQueryable<string> CreateSelectWhereQuery (IQueryable<Chef> source)
    {
      return from s in source where s.Name == "Garcia" select s.FirstName;
    }

    public static IQueryable<Chef> CreateWhereQueryWithDifferentComparisons (IQueryable<Chef> source)
    {
      return from s in source where s.FirstName != "Garcia" && s.ID > 5 && s.ID >= 6 && s.ID < 7 && s.ID <= 6 && s.ID == 6 select s;
    }

    public static IQueryable<Chef> CreateWhereQueryWithOrAndNot (IQueryable<Chef> source)
    {
      return from s in source where (!(s.FirstName == "Garcia") || s.FirstName == "Garcia") && s.FirstName == "Garcia" select s;
    }

    public static IQueryable<Chef> CreateWhereQueryWithStartsWith (IQueryable<Chef> source)
    {
      return from s in source where s.FirstName.StartsWith("Garcia") select s;
    }

    public static IQueryable<Chef> CreateWhereQueryWithEndsWith (IQueryable<Chef> source)
    {
      return from s in source where s.FirstName.EndsWith("Garcia") select s;
    }

    public static IQueryable<Chef> CreateWhereQueryNullChecks (IQueryable<Chef> source)
    {
      return from s in source where s.FirstName == null || null != s.Name select s;
    }

    public static IQueryable<Chef> CreateWhereQueryBooleanConstantTrue (IQueryable<Chef> source)
    {
      return from s in source where true select s;
    }

    public static IQueryable<Chef> CreateWhereQueryBooleanConstantFalse (IQueryable<Chef> source)
    {
      return from s in source where false select s;
    }

    public static MethodCallExpression CreateSimpleWhereQuery_WhereExpression (IQueryable<Chef> source)
    {
      IQueryable<Chef> query = CreateSimpleWhereQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateWhereQueryWithEvaluatableSubExpression_WhereExpression (IQueryable<Chef> source)
    {
      IQueryable<Chef> query = CreateWhereQueryWithEvaluatableSubExpression (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateMultiWhereQuery_WhereExpression (IQueryable<Chef> source)
    {
      IQueryable<Chef> query = CreateMultiWhereQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static IQueryable<Student_Detail> CreateRelationMemberWhereQuery (IQueryable<Student_Detail> source)
    {
      return from sd in source where sd.IndustrialSector != null select sd;
    }

    public static IQueryable<IndustrialSector> CreateRelationMemberVirtualSideWhereQuery (IQueryable<IndustrialSector> source)
    {
      return from industrial in source where industrial.Student_Detail != null select industrial;
    }

    public static MethodCallExpression CreateSelectWhereQuery_SelectExpression (IQueryable<Chef> source)
    {
      IQueryable<string> query = CreateSelectWhereQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static IQueryable<Student_Detail> CreateWhereQueryWithRelatedPrimaryKey_VirtualColumn (IQueryable<Student_Detail> source)
    {
      return from sd in source where sd.Chef.ID == 5 select sd;
    }

    public static IQueryable<Student_Detail> CreateWhereQueryWithRelatedPrimaryKey_RealColumn (IQueryable<Student_Detail> source)
    {
      return from sd in source where sd.IndustrialSector.ID == 5 select sd;
    }
  }
}
