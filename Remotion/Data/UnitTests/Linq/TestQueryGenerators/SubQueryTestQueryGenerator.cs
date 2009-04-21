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
using System.Linq;

namespace Remotion.Data.UnitTests.Linq.TestQueryGenerators
{
  public class SubQueryTestQueryGenerator
  {
    public static IQueryable<Student> CreateSimpleSubQueryInAdditionalFromClause(IQueryable<Student> source)
    {
      return from s in source from s2 in (from s3 in source select s3) select s;
    }

    public static IQueryable<Student> CreateComplexSubQueryInAdditionalFromClause (IQueryable<Student> source)
    {
      return from s in source from s2 in (from s3 in source where s3.ID == s.ID && s3.ID > 3 select s3) select s2;
    }

    public static IQueryable<Student> CreateSimpleSubQueryInWhereClause (IQueryable<Student> source)
    {
      return from s in source where (from s2 in source select s2).Contains (s) select s;
    }

    public static IQueryable<Student> CreateSubQueryWithConstantInWhereClause (IQueryable<Student> source)
    {
      Student student = new Student {ID = 5};
      return from s in source where (from s2 in source select s2).Contains (student) select s;
    }

    

    public static IQueryable<Student> CreateSubQuerySelectingColumnsWithConstantInWhereClause (IQueryable<Student> source)
    {
      return from s in source where (from s2 in source select s2.First).Contains ("Hugo") select s;
    }
  }
}
