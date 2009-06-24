// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Collections;

namespace Remotion.Data.UnitTests.Linq.TestQueryGenerators
{
  public static class SelectTestQueryGenerator
  {
    public static IQueryable<Student> CreateSimpleQueryWithNonDBProjection (IQueryable<Student> source)
    {
      return from s in source select (Student) null;
    }

    public static IQueryable<Tuple<string, string>> CreateSimpleQueryWithFieldProjection (IQueryable<Student> source)
    {
      return from s in source select new Tuple<string, string> (s.First, s.Last);
    }

    public static IQueryable<Tuple<Student, string, string, string>> CreateSimpleQueryWithSpecialProjection (IQueryable<Student> source)
    {
      string k = "Test";
      return from s in source select Tuple.NewTuple (s, s.Last, k, "Test2");
    }

    public static IQueryable<string> CreateSimplifyableQuery (IQueryable<Student> source)
    {
      return from s in source select "1" + GetString();
    }

    private static string GetString ()
    {
      return "2";
    }

    public static IQueryable<string> CreateSimpleQueryWithProjection (IQueryable<Student> source)
    {
      return from s in source select s.First;
    }

    public static IQueryable<string> CreateSimpleSelectWithNonDbProjection (IQueryable<Student> source1)
    {
      return from s1 in source1 select s1.NonDBProperty;
    }

    public static IQueryable<int> CreateSimpleSelectWithNonEntityMemberAccess (IQueryable<Student> source1)
    {
      DateTime now = DateTime.Now;
      return from s1 in source1 select now.Day;
    }

    public static IQueryable<Student> CreateRelationMemberSelectQuery(IQueryable<Student_Detail> source)
    {
      return from sd in source select sd.Student;
    }

    public static MethodCallExpression CreateSimpleQuery_SelectExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateSimpleQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateSubQueryInSelct_SelectExpression (IQueryable<Student> source)
    {
      IQueryable<IQueryable<Student>> query = CreateSubQueryInSelect (source);
      return (MethodCallExpression) query.Expression;
    }

    public static IQueryable<Student> CreateSimpleQuery (IQueryable<Student> source)
    {
      return from s in source select s;
    }

    public static IQueryable<string> CreateSimpleQuery_WithProjection (IQueryable<Student> source)
    {
      return from s in source select s.First;
    }

    public static IQueryable<int> CreateSimpleQueryOnID (IQueryable<Student> source)
    {
      return from s in source select s.ID;
    }

    public static IQueryable<string> CreateUnaryBinaryLambdaInvocationConvertNewArrayExpressionQuery (IQueryable<Student> source1)
    {
      return from s1 in source1 select ((Func<string, string>) (s => s1.First)) (s1.Last) + new[] { s1.ToString () }[s1.ID];
    }

    public static IQueryable<IQueryable<Student>> CreateSubQueryInSelect (IQueryable<Student> source)
    {
      return from s in source select (from o in source select o);
    }

    public static IQueryable<IQueryable<Student>> CreateSubQueryInSelect_WithoutExplicitSelect (IQueryable<Student> source)
    {
      return from s in source select (from o in source where o != null select o);
    }

    public static MethodCallExpression CreateSubQueryInMainFrom (IQueryable<Student> source)
    {
      var query = from s in (from si in source select si) select s;
      return (MethodCallExpression) query.Expression;
    }

    public static Expression CreateCountQueryExpression (IQueryable<Student> source)
    {
      return ExpressionHelper.MakeExpression (() => (from s in source select s).Count());
    }
  }
}
