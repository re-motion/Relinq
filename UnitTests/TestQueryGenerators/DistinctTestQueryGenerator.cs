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
  public static class DistinctTestQueryGenerator
  {
    public static IQueryable<string> CreateSimpleDistinctQuery (IQueryable<Cook> source)
    {
      return (from s in source select s.FirstName).Distinct ();
    }

    public static IQueryable<Cook> CreateDisinctWithWhereQueryWithoutProjection (IQueryable<Cook> source)
    {
      return (from s in source where s.FirstName == "Garcia" select s).Distinct ();
    }

    public static IQueryable<string> CreateDisinctWithWhereQuery (IQueryable<Cook> source)
    {
      return (from s in source where s.FirstName == "Garcia" select s.FirstName).Distinct ();
    }

    public static MethodCallExpression CreateSimpleDistinctQuery_MethodCallExpression (IQueryable<Cook> source)
    {
      IQueryable<string> query = CreateSimpleDistinctQuery (source);
      MethodCallExpression newQuery = (MethodCallExpression) ((MethodCallExpression) query.Expression).Arguments[0];
      return newQuery;
    }
  }
}
