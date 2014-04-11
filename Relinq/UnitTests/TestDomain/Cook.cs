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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.TestDomain
{
  public class Cook : ISpecificCook
  {
    public MetaID KnifeID { get; set; }
    public Knife Knife { get; set; }
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

    public Cook GetSubKitchenCook (Restaurant restaurant)
    {
      return restaurant.SubKitchen.Cook;
    }

    [MethodCallExpressionTransformer (typeof (FullNameTransformer))]
    public virtual string GetFullName ()
    {
      return FirstName + " " + Name;
    }

    public double WeightInLbs
    {
      [MethodCallExpressionTransformer (typeof (ToLbsTransformer))]
      get { return Weight * 2.20462262; }
    }

    public Cook this[int assistantIndex]
    {
      [MethodCallExpressionTransformer (typeof (IndexerTransformer))]
      get { return Assistants.Take (assistantIndex).First(); }
    }

    [MethodCallExpressionTransformer (typeof (AssistantCountTransformer))]
    public int GetAssistantCount ()
    {
      return Assistants.Count ();
    }

    [FullNameEqualsTransformer]
    public bool Equals (Cook other)
    {
      return GetFullName() == other.GetFullName();
    }

    public class FullNameTransformer : IExpressionTransformer<MethodCallExpression>
    {
      public ExpressionType[] SupportedExpressionTypes
      {
        get { throw new NotImplementedException(); }
      }

      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        var concatMethod = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
        return Expression.Add (
            Expression.Add (
                Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("FirstName")),
                Expression.Constant (" "),
                concatMethod),
            Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("Name")),
            concatMethod);
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

    public class IndexerTransformer : IExpressionTransformer<MethodCallExpression>
    {
      public ExpressionType[] SupportedExpressionTypes
      {
        get { throw new NotImplementedException (); }
      }

      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        var assistantsMember = Expression.MakeMemberAccess (methodCallExpression.Object, typeof (Cook).GetProperty ("Assistants"));
        var takenAssistants = Expression.Call (typeof (Enumerable), "Take", new[] { typeof (Cook) }, assistantsMember, methodCallExpression.Arguments.Single());
        return Expression.Call (typeof (Enumerable), "First", new[] { typeof (Cook) }, takenAssistants);
      }
    }

    public class FullNameEqualsTransformerAttribute : Attribute, AttributeEvaluatingExpressionTransformer.IMethodCallExpressionTransformerAttribute
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
