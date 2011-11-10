// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.UnitTests.Linq.Core.TestDomain
{
  public class Cook : ISpecificCook
  {
    public MetaID MetaID { get; set; }
    public string FirstName { get; set; }
    public string Name { get; set; }
    public int ID { get; set; }
    public ArrayList Courses { get; set; }
    public int[] IllnessDays { get; set; }
    public List<int> Holidays { get; set; }
    public IQueryable<Cook> Assistants { get; set; }
    public string NonDBStringProperty { get; set; }
    public bool NonDBBoolProperty { get; set; }
    public bool IsFullTimeCook { get; set; }
    public bool IsStarredCook { get; set; }
    public Cook Substitution { get; set; }
    public double Weight { get; set; }
    public string SpecificInformation { get; set; }

    [MethodCallTransformer (typeof (FullNameTransformer))]
    public virtual string GetFullName ()
    {
      return FirstName + " " + Name;
    }

    public double WeightInLbs
    {
      [MethodCallTransformer (typeof (ToLbsTransformer))]
      get { return Weight * 2.20462262; }
    }

    [MethodCallTransformer (typeof (AssistantCountTransformer))]
    public int GetAssistantCount ()
    {
      return Assistants.Count ();
    }

    [MethodCallTransformer (typeof (FullNameEqualsTransformer))]
    public bool Equals (Cook other)
    {
      return GetFullName () == other.GetFullName ();
    }

    public class FullNameTransformer : IMethodCallTransformer
    {
      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        var concatMethod = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string), typeof (string) });
        return Expression.Call (
            concatMethod,
            Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("FirstName")),
            new SqlLiteralExpression (" "),
            Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("Name")));
      }
    }

    public class ToLbsTransformer : IMethodCallTransformer
    {
      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        return Expression.Multiply (
            Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("Weight")),
            new SqlLiteralExpression (2.20462262));
      }
    }

    public class AssistantCountTransformer : IMethodCallTransformer
    {
      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        var mainFromClause = new MainFromClause (
            "a", 
            typeof (Cook), 
            Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("Assistants")));
        var selectClause = new SelectClause (new QuerySourceReferenceExpression (mainFromClause));
        var countQueryModel = new QueryModel (mainFromClause, selectClause);
        countQueryModel.ResultOperators.Add (new CountResultOperator ());
        return new SubQueryExpression (countQueryModel);
      }
    }

    public class FullNameEqualsTransformer : IMethodCallTransformer
    {
      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        return Expression.Equal (
            Expression.Call (methodCallExpression.Object, "GetFullName", Type.EmptyTypes),
            Expression.Call (methodCallExpression.Arguments[0], "GetFullName", Type.EmptyTypes));
      }
    }
  }
}
