// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System.Linq;
using System.Linq.Expressions;

namespace Remotion.Data.UnitTests.Linq.TestQueryGenerators
{
  public static class WhereTestQueryGenerator
  {
    public static IQueryable<Student> CreateSimpleWhereQuery (IQueryable<Student> source)
    {
      return from s in source where s.Last == "Garcia" select s;
    }

    public static IQueryable<Student> CreateWhereQueryWithEvaluatableSubExpression (IQueryable<Student> source)
    {
      string cia = "cia";
      return from s in source where s.Last == ("Gar" + cia) select s;
    }

    public static IQueryable<Student> CreateMultiWhereQuery (IQueryable<Student> source)
    {
      return from s in source where s.Last == "Garcia" where s.First == "Hugo" where s.ID > 100 select s;
    }

    public static IQueryable<string> CreateSelectWhereQuery (IQueryable<Student> source)
    {
      return from s in source where s.Last == "Garcia" select s.First;
    }

    public static IQueryable<Student> CreateWhereQueryWithDifferentComparisons (IQueryable<Student> source)
    {
      return from s in source where s.First != "Garcia" && s.ID > 5 && s.ID >= 6 && s.ID < 7 && s.ID <= 6 && s.ID == 6 select s;
    }

    public static IQueryable<Student> CreateWhereQueryWithOrAndNot (IQueryable<Student> source)
    {
      return from s in source where (!(s.First == "Garcia") || s.First == "Garcia") && s.First == "Garcia" select s;
    }

    public static IQueryable<Student> CreateWhereQueryWithStartsWith (IQueryable<Student> source)
    {
      return from s in source where s.First.StartsWith("Garcia") select s;
    }

    public static IQueryable<Student> CreateWhereQueryWithEndsWith (IQueryable<Student> source)
    {
      return from s in source where s.First.EndsWith("Garcia") select s;
    }

    public static IQueryable<Student> CreateWhereQueryNullChecks (IQueryable<Student> source)
    {
      return from s in source where s.First == null || null != s.Last select s;
    }

    public static IQueryable<Student> CreateWhereQueryBooleanConstantTrue (IQueryable<Student> source)
    {
      return from s in source where true select s;
    }

    public static IQueryable<Student> CreateWhereQueryBooleanConstantFalse (IQueryable<Student> source)
    {
      return from s in source where false select s;
    }

    public static MethodCallExpression CreateSimpleWhereQuery_WhereExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateSimpleWhereQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateWhereQueryWithEvaluatableSubExpression_WhereExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateWhereQueryWithEvaluatableSubExpression (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateMultiWhereQuery_WhereExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateMultiWhereQuery (source);
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

    public static MethodCallExpression CreateSelectWhereQuery_SelectExpression (IQueryable<Student> source)
    {
      IQueryable<string> query = CreateSelectWhereQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static IQueryable<Student_Detail> CreateWhereQueryWithRelatedPrimaryKey_VirtualColumn (IQueryable<Student_Detail> source)
    {
      return from sd in source where sd.Student.ID == 5 select sd;
    }

    public static IQueryable<Student_Detail> CreateWhereQueryWithRelatedPrimaryKey_RealColumn (IQueryable<Student_Detail> source)
    {
      return from sd in source where sd.IndustrialSector.ID == 5 select sd;
    }
  }
}
