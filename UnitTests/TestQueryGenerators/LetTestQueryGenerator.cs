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
  public static class LetTestQueryGenerator
  {
    public static IQueryable<string> CreateSimpleLetClause (IQueryable<Cook> source)
    {
      return from s in source let x = s.FirstName + s.Name select x;
    }

    public static IQueryable<string> CreateLet_WithJoin_NoTable (IQueryable<Kitchen> source)
    {
      return from sd in source let x = sd.Cook.FirstName select x;
    }

    public static IQueryable<Cook> CreateLet_WithJoin_WithTable (IQueryable<Kitchen> source)
    {
      return from sd in source let x = sd.Cook select x;
    }

    public static IQueryable<Cook> CreateLet_WithTable (IQueryable<Cook> source)
    {
      return from s in source let x = s select x;
    }

    public static IQueryable<string> CreateMultiLet_WithWhere (IQueryable<Cook> source)
    {
      return from s in source let x = s.FirstName let y = s.ID where y > 1 select x;
    }


    public static MethodCallExpression CreateSimpleSelect_LetExpression (IQueryable<Cook> source)
    {
      IQueryable<string> query = CreateSimpleLetClause (source);
      return (MethodCallExpression) query.Expression;
    }
  }
}
