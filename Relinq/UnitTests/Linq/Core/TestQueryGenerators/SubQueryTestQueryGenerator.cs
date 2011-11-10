// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.Core.TestQueryGenerators
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
