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
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.TestQueryGenerators
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
