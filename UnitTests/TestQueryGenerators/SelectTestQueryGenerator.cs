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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.TestQueryGenerators
{
  internal static class SelectTestQueryGenerator
  {
    public static IQueryable<Cook> CreateSimpleQueryWithNonDBProjection (IQueryable<Cook> source)
    {
      return from s in source select (Cook) null;
    }

    public static IQueryable<Tuple<string, string>> CreateSimpleQueryWithFieldProjection (IQueryable<Cook> source)
    {
      return from s in source select new Tuple<string, string> (s.FirstName, s.Name);
    }

    public static IQueryable<Tuple<Cook, string, string, string>> CreateSimpleQueryWithSpecialProjection (IQueryable<Cook> source)
    {
      string k = "Test";
      return from s in source select Tuple.Create (s, s.Name, k, "Test2");
    }

    public static IQueryable<string> CreateSimplifyableQuery (IQueryable<Cook> source)
    {
      return from s in source select "1" + GetString();
    }

    private static string GetString ()
    {
      return "2";
    }

    public static IQueryable<string> CreateSimpleQueryWithProjection (IQueryable<Cook> source)
    {
      return from s in source select s.FirstName;
    }

    public static IQueryable<string> CreateSimpleSelectWithNonDbProjection (IQueryable<Cook> source1)
    {
      return from s1 in source1 select s1.NonDBStringProperty;
    }

    public static IQueryable<int> CreateSimpleSelectWithNonEntityMemberAccess (IQueryable<Cook> source1)
    {
      DateTime now = DateTime.Now;
      return from s1 in source1 select now.Day;
    }

    public static IQueryable<Cook> CreateRelationMemberSelectQuery(IQueryable<Kitchen> source)
    {
      return from sd in source select sd.Cook;
    }

    public static MethodCallExpression CreateSimpleQuery_SelectExpression (IQueryable<Cook> source)
    {
      IQueryable<Cook> query = CreateSimpleQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateSubQueryInSelct_SelectExpression (IQueryable<Cook> source)
    {
      IQueryable<IQueryable<Cook>> query = CreateSubQueryInSelect (source);
      return (MethodCallExpression) query.Expression;
    }

    public static IQueryable<Cook> CreateSimpleQuery (IQueryable<Cook> source)
    {
      return from s in source select s;
    }

    public static IQueryable<string> CreateSimpleQuery_WithProjection (IQueryable<Cook> source)
    {
      return from s in source select s.FirstName;
    }

    public static IQueryable<int> CreateSimpleQueryOnID (IQueryable<Cook> source)
    {
      return from s in source select s.ID;
    }

    public static IQueryable<string> CreateUnaryBinaryLambdaInvocationConvertNewArrayExpressionQuery (IQueryable<Cook> source1)
    {
      return from s1 in source1 select ((Func<string, string>) (s => s1.FirstName)) (s1.Name) + new[] { s1.ToString () }[s1.ID];
    }

    public static IQueryable<IQueryable<Cook>> CreateSubQueryInSelect (IQueryable<Cook> source)
    {
      return from s in source select (from o in source select o);
    }

    public static IQueryable<IQueryable<Cook>> CreateSubQueryInSelect_WithoutExplicitSelect (IQueryable<Cook> source)
    {
      return from s in source select (from o in source where o != null select o);
    }

    public static MethodCallExpression CreateSubQueryInMainFrom (IQueryable<Cook> source)
    {
      var query = from s in (from si in source select si) select s;
      return (MethodCallExpression) query.Expression;
    }

    public static Expression CreateCountQueryExpression (IQueryable<Cook> source)
    {
      return ExpressionHelper.MakeExpression (() => (from s in source select s).Count());
    }
  }
}
