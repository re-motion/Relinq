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
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.TestQueryGenerators
{
  public class SubQueryTestQueryGenerator
  {
    public static IQueryable<Cook> CreateSimpleSubQueryInMainFromClause (IQueryable<Cook> source)
    {
      return from s in (from s2 in source select s2).Take (1) select s;
    }

    public static IQueryable<Cook> CreateSimpleSubQueryInAdditionalFromClause(IQueryable<Cook> source)
    {
      return from s in source from s2 in (from s3 in source select s3) select s;
    }

    public static IQueryable<Cook> CreateComplexSubQueryInAdditionalFromClause (IQueryable<Cook> source)
    {
      return from s in source from s2 in (from s3 in source where s3.ID == s.ID && s3.ID > 3 select s3) select s2;
    }

    public static IQueryable<Cook> CreateSimpleSubQueryInWhereClause (IQueryable<Cook> source)
    {
      return from s in source where (from s2 in source select s2).Contains (s) select s;
    }

    public static IQueryable<Cook> CreateSubQueryWithConstantInWhereClause (IQueryable<Cook> source)
    {
      Cook cook = new Cook {ID = 5};
      return from s in source where (from s2 in source select s2).Contains (cook) select s;
    }

    

    public static IQueryable<Cook> CreateSubQuerySelectingColumnsWithConstantInWhereClause (IQueryable<Cook> source)
    {
      return from s in source where (from s2 in source select s2.FirstName).Contains ("Hugo") select s;
    }
  }
}
