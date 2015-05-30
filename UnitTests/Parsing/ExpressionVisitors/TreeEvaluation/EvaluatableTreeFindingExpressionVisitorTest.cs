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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if !NET_3_5
using Microsoft.CSharp.RuntimeBinder;
#endif
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.Expressions;
#if NET_3_5
using Remotion.Linq.Parsing;
#endif
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.UnitTests.Parsing.ExpressionVisitorTests;
using Remotion.Linq.UnitTests.Parsing.Structure.TestDomain;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;
#if !NET_3_5
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;
#endif

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitors.TreeEvaluation
{
  [TestFixture]
  public class EvaluatableTreeFindingExpressionVisitorTest
  {
#if !NET_3_5
    private class TestExtensionExpressionWithType : Expression
    {
      public TestExtensionExpressionWithType ()
      {
      }

      public override ExpressionType NodeType
      {
        get { return ExpressionType.Extension; }
      }

      public override bool CanReduce
      {
        get { return false; }
      }

      public override string ToString ()
      {
        return "Test(Extension)";
      }

      protected override Expression VisitChildren (ExpressionVisitor visitor)
      {
        return this;
      }
    }
#endif

    [Test]
    public void SimpleExpression_IsEvaluatable ()
    {
      var expression = Expression.Constant (0);
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
    }

    [Test]
    public void NestedExpression_InnerAndOuterAreEvaluatable ()
    {
      var innerExpressionLeft = Expression.Constant (0);
      var innerExpressionRight = Expression.Constant (0);
      var outerExpression = Expression.MakeBinary (ExpressionType.Add, innerExpressionLeft, innerExpressionRight);
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (outerExpression);

      Assert.That (evaluationInfo.IsEvaluatableExpression (outerExpression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpressionLeft), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpressionRight), Is.True);
    }

    [Test]
    public void ParameterExpression_IsNotEvaluatable ()
    {
      var expression = ExpressionHelper.CreateParameterExpression ();
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void ExpressionContainingParameterExpression_IsNotEvaluatable ()
    {
      var expression = Expression.MakeBinary (
          ExpressionType.Equal, 
          ExpressionHelper.CreateParameterExpression (), 
          ExpressionHelper.CreateParameterExpression ());
      
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void ParameterExpression_SiblingCanBeEvaluatable ()
    {
      var expression = Expression.MakeBinary (
          ExpressionType.Equal,
          ExpressionHelper.CreateParameterExpression (),
          Expression.Constant (0));

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.Right), Is.True);
    }

    [Test]
    public void VisitQuerySourceReferenceExpression_NotEvaluatable ()
    {
      var expression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void VisitSubQueryExpression_NotEvaluatable ()
    {
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel<Cook>());
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void VisitExtensionExpression_NotEvaluatable_ButChildrenMayBe ()
    {
      var innerExpression = Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (0), Expression.Constant (0));
      var extensionExpression = new TestExtensionExpression (innerExpression);
#if !NET_3_5
      Assert.That (extensionExpression.NodeType, Is.EqualTo (ExpressionType.Extension));
#endif

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (extensionExpression);

      Assert.That (evaluationInfo.IsEvaluatableExpression (extensionExpression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpression), Is.True);
    }

    [Test]
    public void NullExpression_InOtherExpression_IsIgnored ()
    {
      var expression = Expression.MakeBinary (
          ExpressionType.Equal,
          ExpressionHelper.CreateParameterExpression (),
          ExpressionHelper.CreateParameterExpression ());

      Assert.That (expression.Conversion, Is.Null);

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);
      Assert.That (evaluationInfo.Count, Is.EqualTo (0));
    }

    [Test]
    public void MethodCall_WithIQueryableObject_IsNotEvaluatable ()
    {
      var source = ExpressionHelper.CreateQueryable<Cook> ();
      var expression = ExpressionHelper.MakeExpression (() => source.ToString());

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void MethodCall_WithIQueryableParameter_IsNotEvaluatable ()
    {
      var source = ExpressionHelper.CreateQueryable<Cook> ();
      var expression = ExpressionHelper.MakeExpression (() => source.Count ());

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void MemberExpression_WithIQueryableObject_IsNotEvaluatable ()
    {
      var source = new QueryableFakeWithCount<int>();
      var expression = ExpressionHelper.MakeExpression (() => source.Count);

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void MemberInitialization_WithParametersInMemberAssignments_IsNotEvaluatable ()
    {
      var expression = (MemberInitExpression) ExpressionHelper.MakeExpression<int, AnonymousType> (i => new AnonymousType { a = i, b = 1 });

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.False);
    }

    [Test]
    public void ListInitialization_WithParametersInMemberAssignments_IsNotEvaluatable ()
    {
      var expression = (ListInitExpression) ExpressionHelper.MakeExpression<int, List<int>> (i => new List<int> { i, 1 });

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.False);
    }

    [Test]
    public void MemberInitialization_WithoutParametersInMemberAssignments_IsEvaluatable ()
    {
      var expression = (MemberInitExpression) ExpressionHelper.MakeExpression<int, AnonymousType> (i => new AnonymousType { a = 1, b = 1 });

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.True);
    }

    [Test]
    public void ListInitialization_WithoutParametersInMemberAssignments_IsEvaluatable ()
    {
      var expression = (ListInitExpression) ExpressionHelper.MakeExpression<int, List<int>> (i => new List<int> { 2, 1 });

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.True);
    }

    [Test]
    public void VisitUnknownExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = EvaluatableTreeFindingExpressionVisitor.Analyze (expression);

      Assert.That (result.IsEvaluatableExpression (expression), Is.False);
    }

#if !NET_3_5
    [Test]
    public void VisitDynamicExpression_WithParameterReference_NonEvaluable ()
    {
      var parameterExpression = Expression.Parameter (typeof (object), "x");

      var dynamicExpressionWithParameterReference =
          Expression.Dynamic (
              Binder.GetMember (
                  CSharpBinderFlags.InvokeSimpleName,
                  "colour",
                  typeof (object),
                  new[] { CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null) }),
              typeof (object),
              parameterExpression);

      var body = Expression.MakeBinary (ExpressionType.Equal, dynamicExpressionWithParameterReference, Expression.Constant ("orange"));
      
      var result = EvaluatableTreeFindingExpressionVisitor.Analyze (body);

      Assert.That (result.IsEvaluatableExpression (body), Is.False);
    }
#endif

    [Test]
    public void PartialEvaluationExceptionExpression_NotEvaluable_AndChildrenNeither ()
    {
      var inner = Expression.Constant (0);
      var partialEvaluationExceptionExpression = new PartialEvaluationExceptionExpression (new Exception(), inner);

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (partialEvaluationExceptionExpression);

      Assert.That (evaluationInfo.IsEvaluatableExpression (partialEvaluationExceptionExpression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (inner), Is.False);
    }

    [Test]
    public void VisitEvaluatableStandardExpressions ()
    {
      var supportedExpressionTypeValues =
          Enum.GetValues (typeof (ExpressionType))
              .Cast<ExpressionType>()
              .Except (
                  new[]
                  {
                      ExpressionType.Parameter,
#if !NET_3_5
                      ExpressionType.Extension,
                      ExpressionType.RuntimeVariables,
#endif
                  })
              .ToArray();

      var visitMethodExpressionTypes = new HashSet<Type> (
          from m in typeof (ExpressionVisitor).GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
          where m.Name.StartsWith ("Visit")
          let parameters = m.GetParameters()
          where parameters.Length == 1
          let expressionType = parameters.Single().ParameterType
          where expressionType != typeof (Expression)
          select expressionType);
      Assert.That (visitMethodExpressionTypes.Count > 0);

      foreach (var expressionType in supportedExpressionTypeValues)
      {
        var expressionInstance = ExpressionInstanceCreator.GetExpressionInstance (expressionType);
        if (expressionInstance == null)
          continue;

        Assert.That (
            visitMethodExpressionTypes.Any (
                t =>
                {
                  if (t.ContainsGenericParameters && expressionInstance.GetType().IsGenericType)
                    t = t.GetGenericTypeDefinition().MakeGenericType (expressionInstance.GetType().GetGenericArguments());
                  return t.IsInstanceOfType (expressionInstance);
                }),
            Is.True,
            "Visit method for {0}",
            expressionInstance.GetType());
        var result = EvaluatableTreeFindingExpressionVisitor.Analyze (expressionInstance);
        Assert.That (result.IsEvaluatableExpression (expressionInstance), Is.True, expressionInstance.NodeType.ToString());
      }
    }
  }
}
