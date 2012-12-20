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
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;
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
    public Cook Substituted { get; set; }
    public double Weight { get; set; }
    public string SpecificInformation { get; set; }
    public Kitchen Kitchen { get; set; }

    [MethodCallTransformer (typeof (FullNameSqlTransformer))]
    public virtual string GetFullName_SqlTransformed ()
    {
      return FirstName + " " + Name;
    }

    [MethodCallExpressionTransformer (typeof (FullNameTransformer))]
    public virtual string GetFullName ()
    {
      return FirstName + " " + Name;
    }

    public double WeightInLbs_SqlTransformed
    {
      [MethodCallTransformer (typeof (ToLbsSqlTransformer))]
      get { return Weight * 2.20462262; }
    }

    public double WeightInLbs
    {
      [MethodCallExpressionTransformer (typeof (ToLbsTransformer))]
      get { return Weight * 2.20462262; }
    }

    [MethodCallExpressionTransformer (typeof (AssistantCountTransformer))]
    public int GetAssistantCount ()
    {
      return Assistants.Count ();
    }

    [MethodCallTransformer (typeof (AssistantCountSqlTransformer))]
    public int GetAssistantCount_SqlTransformed ()
    {
      return Assistants.Count ();
    }

    [FullNameEqualsTransformer]
    public bool Equals (Cook other)
    {
      return GetFullName() == other.GetFullName();
    }

    [MethodCallTransformer (typeof (FullNameEqualsSqlTransformer))]
    public bool Equals_SqlTransformed (Cook other)
    {
      return GetFullName_SqlTransformed () == other.GetFullName_SqlTransformed ();
    }

    public class FullNameSqlTransformer : IMethodCallTransformer
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

    public class ToLbsSqlTransformer : IMethodCallTransformer
    {
      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        return Expression.Multiply (
            Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("Weight")),
            new SqlLiteralExpression (2.20462262));
      }
    }

    public class AssistantCountSqlTransformer : IMethodCallTransformer
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

    public class FullNameEqualsSqlTransformer : IMethodCallTransformer
    {
      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        return Expression.Equal (
            Expression.Call (methodCallExpression.Object, "GetFullName_SqlTransformed", Type.EmptyTypes),
            Expression.Call (methodCallExpression.Arguments[0], "GetFullName_SqlTransformed", Type.EmptyTypes));
      }
    }

    public class FullNameTransformer : IExpressionTransformer<MethodCallExpression>
    {
      public ExpressionType[] SupportedExpressionTypes
      {
        get { throw new NotImplementedException(); }
      }

      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        var concatMethod = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string), typeof (string) });
        return Expression.Call (
            concatMethod,
            Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("FirstName")),
            Expression.Constant (" "),
            Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("Name")));
      }
    }

    public class ToLbsTransformer : IExpressionTransformer<MethodCallExpression>
    {
      public ExpressionType[] SupportedExpressionTypes
      {
        get { throw new NotImplementedException(); }
      }

      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        return Expression.Multiply (
            Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("Weight")),
            Expression.Constant (2.20462262));
      }
    }

    public class AssistantCountTransformer : IExpressionTransformer<MethodCallExpression>
    {
      public ExpressionType[] SupportedExpressionTypes
      {
        get { throw new NotImplementedException(); }
      }

      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        var assistantsMember = Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("Assistants"));
        return Expression.Call (typeof (Enumerable), "Count", new[] { typeof (Cook) }, assistantsMember);
      }
    }

    public class FullNameEqualsTransformerAttribute : Attribute, AttributeEvaluatingMethodCallExpressionTransformer.IMethodCallExpressionTransformerProvider
    {
      public class FullNameEqualsTransformer : IExpressionTransformer<MethodCallExpression>
      {
        public ExpressionType[] SupportedExpressionTypes
        {
          get { throw new NotImplementedException(); }
        }

        public Expression Transform (MethodCallExpression methodCallExpression)
        {
          return Expression.Equal (
              Expression.Call (methodCallExpression.Object, "GetFullName", Type.EmptyTypes),
              Expression.Call (methodCallExpression.Arguments[0], "GetFullName", Type.EmptyTypes));
        }
      }

      public IExpressionTransformer<MethodCallExpression> GetExpressionTransformer (MethodCallExpression expression)
      {
        return new FullNameEqualsTransformer();
      }
    }
  }
}
