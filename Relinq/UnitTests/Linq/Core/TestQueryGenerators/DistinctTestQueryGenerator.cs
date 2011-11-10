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
using System.Linq.Expressions;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.Core.TestQueryGenerators
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
