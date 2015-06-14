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
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.Expressions;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.UnitTests.Parsing.ExpressionVisitorTests;
using Remotion.Linq.UnitTests.Parsing.Structure.TestDomain;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;
#if !NET_3_5
using Microsoft.CSharp.RuntimeBinder;
#endif
#if NET_3_5
using Remotion.Linq.Parsing;
#endif
#if !NET_3_5
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

#endif

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitors.TreeEvaluation
{
  [TestFixture]
  public class EvaluatableTreeFindingExpressionVisitorTest
  {
    private class FakeEvaluatableExpressionFilter : EvaluatableExpressionFilterBase
    {
    }

#if !NET_3_5
    private class RecursiveExpression : Expression
    {
      public override bool CanReduce
      {
        get { return true; }
      }

      public override Expression Reduce ()
      {
        // Intentionally wrong implementation. Reduce should never return itself.
        return this;
      }
    }
#endif

    [Test]
    public void SimpleExpression_IsEvaluatable ()
    {
      var expression = Expression.Constant (0);
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
    }

    [Test]
    public void NestedExpression_InnerAndOuterAreEvaluatable ()
    {
      var innerExpressionLeft = Expression.Constant (0);
      var innerExpressionRight = Expression.Constant (0);
      var outerExpression = Expression.MakeBinary (ExpressionType.Add, innerExpressionLeft, innerExpressionRight);
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (outerExpression, new FakeEvaluatableExpressionFilter());

      Assert.That (evaluationInfo.IsEvaluatableExpression (outerExpression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpressionLeft), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpressionRight), Is.True);
    }

    [Test]
    public void ParameterExpression_IsNotEvaluatable ()
    {
      var expression = ExpressionHelper.CreateParameterExpression();
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void ExpressionContainingParameterExpression_IsNotEvaluatable ()
    {
      var expression = Expression.MakeBinary (
          ExpressionType.Equal,
          ExpressionHelper.CreateParameterExpression(),
          ExpressionHelper.CreateParameterExpression());

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void ParameterExpression_SiblingCanBeEvaluatable ()
    {
      var expression = Expression.MakeBinary (
          ExpressionType.Equal,
          ExpressionHelper.CreateParameterExpression(),
          Expression.Constant (0));

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.Right), Is.True);
    }

    [Test]
    public void VisitQuerySourceReferenceExpression_NotEvaluatable ()
    {
      var expression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void VisitSubQueryExpression_NotEvaluatable ()
    {
      var expression = new SubQueryExpression (ExpressionHelper.CreateQueryModel<Cook>());
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

#if !NET_3_5
    [Test]
    public void VisitReducibleExtensionExpression_IsEvaluatable ()
    {
      var innerExpression = Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (0), Expression.Constant (0));
      var expression = new ReducibleExtensionExpression (innerExpression);

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpression), Is.True);
    }

    [Test]
    public void VisitNestedReducibleExtensionExpression_IsEvaluatable ()
    {
      var innerExpression = new ReducibleExtensionExpression (Expression.Constant (1));
      var expression = new ReducibleExtensionExpression (Expression.Add (innerExpression, Expression.Constant (1)));

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpression), Is.True);
    }
#else
    [Test]
    public void VisitReducibleExtensionExpression_NotEvaluatable_ButChildrenMayBe ()
    {
      var innerExpression = Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (0), Expression.Constant (0));
      var extensionExpression = new ReducibleExtensionExpression (innerExpression);
      
      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (extensionExpression, new FakeEvaluatableExpressionFilter());

      Assert.That (evaluationInfo.IsEvaluatableExpression (extensionExpression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpression), Is.True);
    }
#endif

    [Test]
    public void VisitNonReducibleExtensionExpression_NotEvaluatable_ButChildrenMayBe ()
    {
      var innerExpression = Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (0), Expression.Constant (0));
      var extensionExpression = new NonReducibleExtensionExpression (innerExpression);

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (extensionExpression, new FakeEvaluatableExpressionFilter());

      Assert.That (evaluationInfo.IsEvaluatableExpression (extensionExpression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpression), Is.True);
    }

    [Test]
    public void VisitNestedNonReducibleExtensionExpression_NotEvaluatable ()
    {
      var innerExpression = new NonReducibleExtensionExpression (Expression.Constant (1));
      var expression = new ReducibleExtensionExpression (Expression.Add (innerExpression, Expression.Constant (1)));

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (innerExpression), Is.False);
    }

#if !NET_3_5
    [Test]
    public void VisitExtensionExpressionWithInfiniteReduceImplementation_Throws ()
    {
      var expression = new RecursiveExpression();

      Assert.That (
          () => EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter()),
          Throws.InvalidOperationException);
    }
#endif

    [Test]
    public void NullExpression_InOtherExpression_IsIgnored ()
    {
      var expression = Expression.MakeBinary (
          ExpressionType.Equal,
          ExpressionHelper.CreateParameterExpression(),
          ExpressionHelper.CreateParameterExpression());

      Assert.That (expression.Conversion, Is.Null);

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());
      Assert.That (evaluationInfo.Count, Is.EqualTo (0));
    }

    [Test]
    public void MethodCall_WithIQueryableObject_IsNotEvaluatable ()
    {
      var source = ExpressionHelper.CreateQueryable<Cook>();
      var expression = ExpressionHelper.MakeExpression (() => source.ToString());

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void MethodCall_WithIQueryableParameter_IsNotEvaluatable ()
    {
      var source = ExpressionHelper.CreateQueryable<Cook>();
      var expression = ExpressionHelper.MakeExpression (() => source.Count());

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void MemberExpression_WithIQueryableObject_IsNotEvaluatable ()
    {
      var source = new QueryableFakeWithCount<int>();
      var expression = ExpressionHelper.MakeExpression (() => source.Count);

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
    }

    [Test]
    public void MemberInitialization_WithParametersInMemberAssignments_IsNotEvaluatable ()
    {
      var expression = (MemberInitExpression) ExpressionHelper.MakeExpression<int, AnonymousType> (i => new AnonymousType { a = i, b = 1 });

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.False);
    }

    [Test]
    public void ListInitialization_WithParametersInMemberAssignments_IsNotEvaluatable ()
    {
      var expression = (ListInitExpression) ExpressionHelper.MakeExpression<int, List<int>> (i => new List<int> { i, 1 });

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.False);
    }

    [Test]
    public void MemberInitialization_WithoutParametersInMemberAssignments_IsEvaluatable ()
    {
      var expression = (MemberInitExpression) ExpressionHelper.MakeExpression<int, AnonymousType> (i => new AnonymousType { a = 1, b = 1 });

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.True);
    }

    [Test]
    public void ListInitialization_WithoutParametersInMemberAssignments_IsEvaluatable ()
    {
      var expression = (ListInitExpression) ExpressionHelper.MakeExpression<int, List<int>> (i => new List<int> { 2, 1 });

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression.NewExpression), Is.True);
    }

    [Test]
    public void VisitUnknownExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());

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

      var result = EvaluatableTreeFindingExpressionVisitor.Analyze (body, new FakeEvaluatableExpressionFilter());

      Assert.That (result.IsEvaluatableExpression (body), Is.False);
    }
#endif

    [Test]
    public void PartialEvaluationExceptionExpression_NotEvaluable_AndChildrenNeither ()
    {
      var inner = Expression.Constant (0);
      var expression = new PartialEvaluationExceptionExpression (new Exception(), inner);

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, new FakeEvaluatableExpressionFilter());

      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
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
        var result = EvaluatableTreeFindingExpressionVisitor.Analyze (expressionInstance, new FakeEvaluatableExpressionFilter());
        Assert.That (result.IsEvaluatableExpression (expressionInstance), Is.True, expressionInstance.NodeType.ToString());
      }
    }


    [Test]
    public void VisitBinary_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var left = Expression.Constant (1);
      var right = Expression.Constant (2);
      var expression = Expression.MakeBinary (ExpressionType.Add, left, right);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableBinary (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (left)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (right)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (left), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (right), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitBinary_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var left = Expression.Constant (1);
      var right = Expression.Constant (2);
      var expression = Expression.MakeBinary (ExpressionType.Add, left, right);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableBinary (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (left)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (right)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (left), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (right), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitBinary_WithLeftOperandNotEvaluatable_ReturnsFalseForExpressionAndLeftOperand ()
    {
      var left = Expression.Constant (1);
      var right = Expression.Constant (2);
      var expression = Expression.MakeBinary (ExpressionType.Add, left, right);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableBinary (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (left)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (right)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (left), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (right), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitBinary_WithRightOperandNotEvaluatable_ReturnsFalseForExpressionAndRightOperand ()
    {
      var left = Expression.Constant (1);
      var right = Expression.Constant (2);
      var expression = Expression.MakeBinary (ExpressionType.Add, left, right);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableBinary (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (left)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (right)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (left), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (right), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitConditional_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var test = Expression.Constant (true);
      var whenTrue = Expression.Constant (1);
      var whenFalse = Expression.Constant (2);
      var expression = Expression.Condition (test, whenTrue, whenFalse);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableConditional (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (test)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (whenTrue)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (whenFalse)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (test), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (whenTrue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (whenFalse), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitConditional_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var test = Expression.Constant (true);
      var whenTrue = Expression.Constant (1);
      var whenFalse = Expression.Constant (2);
      var expression = Expression.Condition (test, whenTrue, whenFalse);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableConditional (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (test)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (whenTrue)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (whenFalse)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (test), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (whenTrue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (whenFalse), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitConditional_WithTestNotEvaluatable_ReturnsFalseForExpressionAndTest ()
    {
      var test = Expression.Constant (true);
      var whenTrue = Expression.Constant (1);
      var whenFalse = Expression.Constant (2);
      var expression = Expression.Condition (test, whenTrue, whenFalse);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableConditional (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (test)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (whenTrue)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (whenFalse)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (test), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (whenTrue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (whenFalse), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitConditional_WithTrueResultNotEvaluatable_ReturnsFalseForExpressionAndTrueResult ()
    {
      var test = Expression.Constant (true);
      var whenTrue = Expression.Constant (1);
      var whenFalse = Expression.Constant (2);
      var expression = Expression.Condition (test, whenTrue, whenFalse);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableConditional (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (test)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (whenTrue)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (whenFalse)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (test), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (whenTrue), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (whenFalse), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitConditional_WithFalseResultNotEvaluatable_ReturnsFalseForExpressionAndFalseResult ()
    {
      var test = Expression.Constant (true);
      var whenTrue = Expression.Constant (1);
      var whenFalse = Expression.Constant (2);
      var expression = Expression.Condition (test, whenTrue, whenFalse);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableConditional (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (test)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (whenTrue)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (whenFalse)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (test), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (whenTrue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (whenFalse), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitConstant_IsEvaluatable_ReturnsTrue ()
    {
      var expression = Expression.Constant (0);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableConstant (expression)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitConstant_IsNotEvaluatable_ReturnsFalse ()
    {
      var expression = Expression.Constant (0);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableConstant (expression)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitElementInit_IsEvaluatable_ReturnsTrueForElementInit ()
    {
      var elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = (ConstantExpression) elementInit.Arguments[0];
      var expression = Expression.ListInit (Expression.New (typeof (List<int>).GetConstructor (new Type[0])), elementInit);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableListInit (expression)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableNew (expression.NewExpression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableElementInit (elementInit)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitElementInit_IsNotEvaluatable_ReturnsFalseForElementInit ()
    {
      var elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = (ConstantExpression) elementInit.Arguments[0];
      var expression = Expression.ListInit (Expression.New (typeof (List<int>).GetConstructor (new Type[0])), elementInit);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableListInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableElementInit (elementInit)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitElementInit_WithArgumentNotEvaluatable_ReturnsFalseForElementInitAndArgument ()
    {
      var elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = (ConstantExpression) elementInit.Arguments[0];
      var expression = Expression.ListInit (Expression.New (typeof (List<int>).GetConstructor (new Type[0])), elementInit);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableListInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableElementInit (elementInit)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitInvocation_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (InvocationExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Invoke);
      var lambda = (LambdaExpression) expression.Expression;
      var body = (ConstantExpression) lambda.Body;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableInvocation (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableLambda (lambda)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (lambda), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitInvocation_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (InvocationExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Invoke);
      var lambda = (LambdaExpression) expression.Expression;
      var body = (ConstantExpression) lambda.Body;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableInvocation (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableLambda (lambda)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (lambda), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitInvocation_WithLambdaNotEvaluatable_ReturnsFalseForExpressionAndLambda ()
    {
      var expression = (InvocationExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Invoke);
      var lambda = (LambdaExpression) expression.Expression;
      var body = (ConstantExpression) lambda.Body;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableInvocation (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableLambda (lambda)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (lambda), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitInvocation_WithArgumentNotEvaluatable_ReturnsFalseForExpressionAndArgument ()
    {
      var expression = ExpressionInstanceCreator.CreateInvokeWithArguments();
      var lambda = (LambdaExpression) expression.Expression;
      var body = (ConstantExpression) lambda.Body;
      var argument = (ConstantExpression) expression.Arguments.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableInvocation (expression)).Repeat.Never();
      // InvocationExpression with arguments always uses LambdaExpression with Parameters, which can never be evaluated.
      filterMock.Stub (_ => _.IsEvaluatableLambda (lambda)).Repeat.Never();
      filterMock.Stub (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      // InvocationExpression with arguments always uses LambdaExpression with Parameters, which can never be evaluated.
      Assert.That (evaluationInfo.IsEvaluatableExpression (lambda), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitLambda_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (LambdaExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Lambda);
      var body = (ConstantExpression) expression.Body;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableLambda (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitLambda_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (LambdaExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Lambda);
      var body = (ConstantExpression) expression.Body;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableLambda (expression)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitLambda_WithBodyNotEvaluatable_ReturnsFalseForExpressionAndLambda ()
    {
      var expression = (LambdaExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Lambda);
      var body = (ConstantExpression) expression.Body;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableLambda (expression)).Repeat.Never();
      filterMock.Stub (_ => _.IsEvaluatableConstant (body)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (body), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitLambda_WithParameters_ReturnsFalseForExpressionAndParameter ()
    {
      var expression = ExpressionInstanceCreator.CreateLambdaWithArguments();
      var body = (ConstantExpression) expression.Body;
      var parameter = expression.Parameters.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableLambda (expression)).Repeat.Never();
      filterMock.Stub (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (body), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (parameter), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitListInitialization_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var newExpression = Expression.New (typeof (List<int>).GetConstructor (new Type[0]));
      var elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = (ConstantExpression) elementInit.Arguments[0];
      var expression = Expression.ListInit (newExpression, elementInit);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableListInit (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableNew (newExpression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableElementInit (elementInit)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (newExpression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitListInitialization_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var newExpression = Expression.New (typeof (List<int>).GetConstructor (new Type[0]));
      var elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = (ConstantExpression) elementInit.Arguments[0];
      var expression = Expression.ListInit (newExpression, elementInit);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableListInit (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableNew (newExpression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableElementInit (elementInit)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (newExpression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitListInitialization_WithNewNotEvaluatable_ReturnsFalseForExpressionAndNewExpression ()
    {
      var newExpression = Expression.New (typeof (List<int>).GetConstructor (new Type[0]));
      var elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = (ConstantExpression) elementInit.Arguments[0];
      var expression = Expression.ListInit (newExpression, elementInit);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableListInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableNew (newExpression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableElementInit (elementInit)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (newExpression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitListInitialization_WithArgumentNotEvaluatable_ReturnsFalseForExpressionAndNewExpressionAndArgumentExpression ()
    {
      var newExpression = Expression.New (typeof (List<int>).GetConstructor (new Type[0]));
      var elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = (ConstantExpression) elementInit.Arguments[0];
      var expression = Expression.ListInit (newExpression, elementInit);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableListInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (false);
      // ElementInit is not evaluated if Argument is not evaluable
      filterMock.Expect (_ => _.IsEvaluatableElementInit (elementInit)).Repeat.Never();
      // NewExpression is not evaluated if Argument is not evaluable
      filterMock.Expect (_ => _.IsEvaluatableNew (newExpression)).Repeat.Never();
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      // NewExpression is not evaluated if Argument is not evaluable
      Assert.That (evaluationInfo.IsEvaluatableExpression (newExpression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitListInitialization_WithElementInitNotEvaluatable_ReturnsFalseForExpressionAndNewExpression ()
    {
      var newExpression = Expression.New (typeof (List<int>).GetConstructor (new Type[0]));
      var elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = (ConstantExpression) elementInit.Arguments[0];
      var expression = Expression.ListInit (newExpression, elementInit);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableListInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableElementInit (elementInit)).Return (false);
      // NewExpression is not evaluated if ElementInit is not evaluable
      filterMock.Expect (_ => _.IsEvaluatableNew (newExpression)).Repeat.Never();
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      // NewExpression is not evaluated if ElementInit is not evaluable
      Assert.That (evaluationInfo.IsEvaluatableExpression (newExpression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitMember_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (MemberExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberAccess);
      var objectExpression = (ConstantExpression) expression.Expression;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableMember (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (objectExpression)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (objectExpression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMember_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (MemberExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberAccess);
      var objectExpression = (ConstantExpression) expression.Expression;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableMember (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (objectExpression)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (objectExpression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMember_WithObjectExpressionNotEvaluatable_ReturnsFalseForExpressionAndObjectExpression ()
    {
      var expression = (MemberExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberAccess);
      var objectExpression = (ConstantExpression) expression.Expression;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableMember (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (objectExpression)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (objectExpression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitMemberAssignment_IsEvaluatable_ReturnsTrueForMemberInit ()
    {
      var memberAssignment = ExpressionInstanceCreator.CreateMemberAssignment();
      var argument = (ConstantExpression) memberAssignment.Expression;
      var expression = Expression.MemberInit (Expression.New (typeof (List<int>).GetConstructor (new Type[0])), memberAssignment);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableMemberInit (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableMemberAssignment (memberAssignment)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableNew (expression.NewExpression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberAssignment_IsNotEvaluatable_ReturnsFalseForMemberInit ()
    {
      var memberAssignment = ExpressionInstanceCreator.CreateMemberAssignment();
      var argument = (ConstantExpression) memberAssignment.Expression;
      var expression = Expression.MemberInit (Expression.New (typeof (List<int>).GetConstructor (new Type[0])), memberAssignment);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableMemberInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableMemberAssignment (memberAssignment)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableNew (expression.NewExpression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberAssignment_WithArgumentNotEvaluatable_ReturnsFalseForMemberAssignmentAndArgument ()
    {
      var memberAssignment = ExpressionInstanceCreator.CreateMemberAssignment();
      var argument = (ConstantExpression) memberAssignment.Expression;
      var expression = Expression.MemberInit (Expression.New (typeof (List<int>).GetConstructor (new Type[0])), memberAssignment);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableMemberInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableMemberAssignment (memberAssignment)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitMemberListBinding_IsEvaluatable_ReturnsTrueForMemberInit ()
    {
      var elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = (ConstantExpression) elementInit.Arguments.Single();
      var memberListBinding = ExpressionInstanceCreator.CreateMemberListBinding (new[] { elementInit });
      var expression = Expression.MemberInit (Expression.New (typeof (SimpleClass).GetConstructor (new Type[0])), memberListBinding);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableMemberInit (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableMemberListBinding (memberListBinding)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableElementInit (elementInit)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableNew (expression.NewExpression)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberListBinding_IsNotEvaluatable_ReturnsFalseForMemberInit ()
    {
      var elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = (ConstantExpression) elementInit.Arguments.Single();
      var memberListBinding = ExpressionInstanceCreator.CreateMemberListBinding (new[] { elementInit });
      var expression = Expression.MemberInit (Expression.New (typeof (SimpleClass).GetConstructor (new Type[0])), memberListBinding);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableMemberInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableMemberListBinding (memberListBinding)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableElementInit (elementInit)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableNew (expression.NewExpression)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberListBinding_WithElementInitNotEvaluatable_ReturnsFalseForMemberListBinding ()
    {
      var elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = (ConstantExpression) elementInit.Arguments.Single();
      var memberListBinding = ExpressionInstanceCreator.CreateMemberListBinding (new[] { elementInit });
      var expression = Expression.MemberInit (Expression.New (typeof (SimpleClass).GetConstructor (new Type[0])), memberListBinding);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableMemberInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableMemberListBinding (memberListBinding)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableElementInit (elementInit)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitMemberMemberBinding_IsEvaluatable_ReturnsTrueForMemberInit ()
    {
      var memberInit = ExpressionInstanceCreator.CreateMemberAssignment();
      var argument = (ConstantExpression) memberInit.Expression;
      var memberMemberBinding = ExpressionInstanceCreator.CreateMemberMemberBinding (new[] { memberInit });
      var expression = Expression.MemberInit (Expression.New (typeof (SimpleClass).GetConstructor (new Type[0])), memberMemberBinding);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableMemberInit (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableMemberMemberBinding (memberMemberBinding)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableMemberAssignment (memberInit)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableNew (expression.NewExpression)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberMemberBinding_IsNotEvaluatable_ReturnsFalseForMemberInit ()
    {
      var memberInit = ExpressionInstanceCreator.CreateMemberAssignment();
      var argument = (ConstantExpression) memberInit.Expression;
      var memberMemberBinding = ExpressionInstanceCreator.CreateMemberMemberBinding (new[] { memberInit });
      var expression = Expression.MemberInit (Expression.New (typeof (SimpleClass).GetConstructor (new Type[0])), memberMemberBinding);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableMemberInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableMemberMemberBinding (memberMemberBinding)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableMemberAssignment (memberInit)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableNew (expression.NewExpression)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberMemberBinding_WithMemberInitNotEvaluatable_ReturnsFalseForMemberMemberBinding ()
    {
      var memberInit = ExpressionInstanceCreator.CreateMemberAssignment();
      var argument = (ConstantExpression) memberInit.Expression;
      var memberMemberBinding = ExpressionInstanceCreator.CreateMemberMemberBinding (new[] { memberInit });
      var expression = Expression.MemberInit (Expression.New (typeof (SimpleClass).GetConstructor (new Type[0])), memberMemberBinding);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableMemberInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableMemberMemberBinding (memberMemberBinding)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableMemberAssignment (memberInit)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitMemberInit_IsEvaluatable_ReturnsTrueForMemberInit ()
    {
      var newExpression = Expression.New (typeof (List<int>).GetConstructor (new Type[0]));
      var memberAssignment = ExpressionInstanceCreator.CreateMemberAssignment();
      var argument = (ConstantExpression) memberAssignment.Expression;
      var expression = Expression.MemberInit (newExpression, memberAssignment);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableMemberInit (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableNew (newExpression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableMemberAssignment (memberAssignment)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (newExpression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberInit_IsNotEvaluatable_ReturnsFalseForMemberInit ()
    {
      var newExpression = Expression.New (typeof (List<int>).GetConstructor (new Type[0]));
      var memberAssignment = ExpressionInstanceCreator.CreateMemberAssignment();
      var argument = (ConstantExpression) memberAssignment.Expression;
      var expression = Expression.MemberInit (newExpression, memberAssignment);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableMemberInit (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableNew (newExpression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableMemberAssignment (memberAssignment)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (newExpression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberInit_WithMemberAssignmentNotEvaluatable_ReturnsFalseForMemberInitAndNew ()
    {
      var newExpression = Expression.New (typeof (List<int>).GetConstructor (new Type[0]));
      var memberAssignment = ExpressionInstanceCreator.CreateMemberAssignment();
      var argument = (ConstantExpression) memberAssignment.Expression;
      var expression = Expression.MemberInit (newExpression, memberAssignment);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableMemberInit (expression)).Repeat.Never();
      // NewExpression is not evaluated if Argument is not evaluable
      filterMock.Stub (_ => _.IsEvaluatableNew (newExpression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableMemberAssignment (memberAssignment)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      // NewExpression is not evaluated if Argument is not evaluable
      Assert.That (evaluationInfo.IsEvaluatableExpression (newExpression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMemberInit_WithNewNotEvaluatable_ReturnsFalseForMemberInitAndNew ()
    {
      var newExpression = Expression.New (typeof (List<int>).GetConstructor (new Type[0]));
      var memberAssignment = ExpressionInstanceCreator.CreateMemberAssignment();
      var argument = (ConstantExpression) memberAssignment.Expression;
      var expression = Expression.MemberInit (newExpression, memberAssignment);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableMemberInit (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableNew (newExpression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableMemberAssignment (memberAssignment)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (newExpression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitMethodCall_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var instance = Expression.Constant (1);
      var argument = Expression.Constant (0);
      var expression = Expression.Call (instance, typeof (int).GetMethod ("Equals", new[] { typeof (int) }), argument);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableMethodCall (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (instance)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (instance), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMethodCall_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var instance = Expression.Constant (1);
      var argument = Expression.Constant (0);
      var expression = Expression.Call (instance, typeof (int).GetMethod ("Equals", new[] { typeof (int) }), argument);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableMethodCall (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (instance)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (instance), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMethodCall_WithInstancetNotEvaluatable_ReturnsFalseForExpressionAndInstance ()
    {
      var instance = Expression.Constant (1);
      var argument = Expression.Constant (0);
      var expression = Expression.Call (instance, typeof (int).GetMethod ("Equals", new[] { typeof (int) }), argument);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableMethodCall (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (instance)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (instance), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitMethodCall_WithArgumentNotEvaluatable_ReturnsFalseForExpressionAndArgument ()
    {
      var instance = Expression.Constant (1);
      var argument = Expression.Constant (0);
      var expression = Expression.Call (instance, typeof (int).GetMethod ("Equals", new[] { typeof (int) }), argument);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableMethodCall (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (instance)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (instance), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitNew_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);
      var argument = (ConstantExpression) expression.Arguments.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableNew (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitNew_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);
      var argument = (ConstantExpression) expression.Arguments.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableNew (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitNew_WithArgumentNotEvaluatable_ReturnsFalseForExpressionAndArgument ()
    {
      var expression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);
      var argument = (ConstantExpression) expression.Arguments.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableNew (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitTypeNewArray_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayInit);
      var argument = (ConstantExpression) expression.Expressions.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableNewArray (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitNewArray_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayInit);
      var argument = (ConstantExpression) expression.Expressions.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableNewArray (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitNewArray_WithArgumentNotEvaluatable_ReturnsFalseForExpressionAndArgument ()
    {
      var expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayInit);
      var argument = (ConstantExpression) expression.Expressions.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableNewArray (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitTypeTypeBinary_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (TypeBinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.TypeIs);
      var argument = (ConstantExpression) expression.Expression;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableTypeBinary (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitTypeBinary_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (TypeBinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.TypeIs);
      var argument = (ConstantExpression) expression.Expression;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableTypeBinary (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitTypeBinary_WithArgumentNotEvaluatable_ReturnsFalseForExpressionAndArgument ()
    {
      var expression = (TypeBinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.TypeIs);
      var argument = (ConstantExpression) expression.Expression;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableTypeBinary (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (argument)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitUnary_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.UnaryPlus);
      var operand = (ConstantExpression) expression.Operand;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableUnary (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (operand)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (operand), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitUnary_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.UnaryPlus);
      var operand = (ConstantExpression) expression.Operand;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableUnary (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (operand)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (operand), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitUnary_WithOperandNotEvaluatable_ReturnsFalseForExpressionAndOperand ()
    {
      var expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.UnaryPlus);
      var operand = (ConstantExpression) expression.Operand;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableUnary (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (operand)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (operand), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

#if !NET_3_5
    [Test]
    public void VisitBlock_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (BlockExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Block);
      var body = (ConstantExpression) expression.Expressions.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableBlock (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (body), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitBlock_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (BlockExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Block);
      var body = (ConstantExpression) expression.Expressions.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableBlock (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (body), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitBlock_WithBodyNotEvaluatable_ReturnsFalseForExpressionAndBody ()
    {
      var expression = (BlockExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Block);
      var body = (ConstantExpression) expression.Expressions.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableBlock (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (body)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (body), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitCatchBlock_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var body = Expression.Constant (1);
      var catchBlock = ExpressionInstanceCreator.CreateCatchBlock();
      var catchBody = (ConstantExpression) catchBlock.Body;
      var expression = Expression.TryCatch (body, catchBlock);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableTry (expression)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (catchBody)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableCatchBlock (catchBlock)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (catchBody), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitCatchBlock_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var body = Expression.Constant (1);
      var catchBlock = ExpressionInstanceCreator.CreateCatchBlock();
      var catchBody = (ConstantExpression) catchBlock.Body;
      var expression = Expression.TryCatch (body, catchBlock);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableTry (expression)).Repeat.Never();
      filterMock.Stub (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (catchBody)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableCatchBlock (catchBlock)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (catchBody), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitCatchBlock_WithCatchBodyNotEvaluatable_ReturnsFalseForExpressionAndCatchBody ()
    {
      var body = Expression.Constant (1);
      var catchBlock = ExpressionInstanceCreator.CreateCatchBlock();
      var catchBody = (ConstantExpression) catchBlock.Body;
      var expression = Expression.TryCatch (body, catchBlock);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableTry (expression)).Repeat.Never();
      filterMock.Stub (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (catchBody)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableCatchBlock (catchBlock)).Repeat.Never();
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (catchBody), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitDebugInfo_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (DebugInfoExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.DebugInfo);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableDebugInfo (expression)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitDebugInfo_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (DebugInfoExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.DebugInfo);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableDebugInfo (expression)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitDefault_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (DefaultExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Default);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableDefault (expression)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitDefault_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (DefaultExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Default);

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableDefault (expression)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitGoto_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (GotoExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Goto);
      var value = (ConstantExpression) expression.Value;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableGoto (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (value)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableLabelTarget (expression.Target)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (value), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitGoto_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (GotoExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Goto);
      var value = (ConstantExpression) expression.Value;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableGoto (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (value)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableLabelTarget (expression.Target)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (value), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitGoto_WithValueNotEvaluatable_ReturnsFalseForExpressionAndValue ()
    {
      var expression = (GotoExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Goto);
      var value = (ConstantExpression) expression.Value;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableGoto (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (value)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableLabelTarget (expression.Target)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (value), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitIndex_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (IndexExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Index);
      var objectExpression = (NewArrayExpression) expression.Object;
      var argument = (ConstantExpression) expression.Arguments.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableIndex (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableNewArray (objectExpression)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (objectExpression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitIndex_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (IndexExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Index);
      var objectExpression = (NewArrayExpression) expression.Object;
      var argument = (ConstantExpression) expression.Arguments.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableIndex (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableNewArray (objectExpression)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (objectExpression), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitIndex_WithObjectNotEvaluatable_ReturnsFalseForExpressionAndObject ()
    {
      var expression = (IndexExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Index);
      var objectExpression = (NewArrayExpression) expression.Object;
      var argument = (ConstantExpression) expression.Arguments.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableIndex (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableNewArray (objectExpression)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableConstant (argument)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (objectExpression), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (argument), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitLabel_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (LabelExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Label);
      var defaultValue = (ConstantExpression) expression.DefaultValue;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableLabel (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (defaultValue)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableLabelTarget (expression.Target)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (defaultValue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitLabel_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (LabelExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Label);
      var defaultValue = (ConstantExpression) expression.DefaultValue;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableLabel (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (defaultValue)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableLabelTarget (expression.Target)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (defaultValue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitLabel_WithDefaultValueNotEvaluatable_ReturnsFalseForExpressionAndDefaultValue ()
    {
      var expression = (LabelExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Label);
      var defaultValue = (ConstantExpression) expression.DefaultValue;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableLabel (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (defaultValue)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableLabelTarget (expression.Target)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (defaultValue), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitLabelTarget_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (LabelExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Label);
      var labelTarget = expression.Target;
      var defaultValue = (ConstantExpression) expression.DefaultValue;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableLabel (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableLabelTarget (labelTarget)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (defaultValue)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (defaultValue), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitLabelTarget_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (LabelExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Label);
      var labelTarget = expression.Target;
      var defaultValue = (ConstantExpression) expression.DefaultValue;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableLabel (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableLabelTarget (labelTarget)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableConstant (defaultValue)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (defaultValue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitLoop_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (LoopExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Loop);
      var body = (ConstantExpression) expression.Body;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableLoop (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (body), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitLoop_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (LoopExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Loop);
      var body = (ConstantExpression) expression.Body;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableLoop (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (body), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitLoop_WithBodyNotEvaluatable_ReturnsFalseForExpressionAndBody ()
    {
      var expression = (LoopExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Loop);
      var body = (ConstantExpression) expression.Body;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableLoop (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (body)).Return (false);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (body), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitSwitch_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (SwitchExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Switch);
      var switchValue = (ConstantExpression) expression.SwitchValue;
      var switchCase = expression.Cases.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableSwitch (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (switchValue)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableSwitchCase (switchCase)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableDefault ((DefaultExpression) switchCase.Body)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant ((ConstantExpression) switchCase.TestValues.Single())).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (switchValue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSwitch_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (SwitchExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Switch);
      var switchValue = (ConstantExpression) expression.SwitchValue;
      var switchCase = expression.Cases.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableSwitch (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (switchValue)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableSwitchCase (switchCase)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableDefault ((DefaultExpression) switchCase.Body)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant ((ConstantExpression) switchCase.TestValues.Single())).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (switchValue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSwitch_WithValueNotEvaluatable_ReturnsFalseForExpressionAndValue ()
    {
      var expression = (SwitchExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Switch);
      var switchValue = (ConstantExpression) expression.SwitchValue;
      var switchCase = expression.Cases.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableSwitch (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (switchValue)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableSwitchCase (switchCase)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableDefault ((DefaultExpression) switchCase.Body)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant ((ConstantExpression) switchCase.TestValues.Single())).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (switchValue), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitSwitchCase_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (SwitchExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Switch);
      var switchValue = (ConstantExpression) expression.SwitchValue;
      var switchCase = expression.Cases.Single();
      var caseBody = (DefaultExpression) switchCase.Body;
      var testValue = (ConstantExpression) switchCase.TestValues.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableSwitch (expression)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (switchValue)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableSwitchCase (switchCase)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableDefault (caseBody)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (testValue)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (caseBody), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (testValue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSwitchCase_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (SwitchExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Switch);
      var switchValue = (ConstantExpression) expression.SwitchValue;
      var switchCase = expression.Cases.Single();
      var caseBody = (DefaultExpression) switchCase.Body;
      var testValue = (ConstantExpression) switchCase.TestValues.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableSwitch (expression)).Repeat.Never();
      filterMock.Stub (_ => _.IsEvaluatableConstant (switchValue)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableSwitchCase (switchCase)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableDefault (caseBody)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (testValue)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (caseBody), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (testValue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitSwitchCase_WithValueNotEvaluatable_ReturnsFalseForExpressionAndValue ()
    {
      var expression = (SwitchExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Switch);
      var switchValue = (ConstantExpression) expression.SwitchValue;
      var switchCase = expression.Cases.Single();
      var caseBody = (DefaultExpression) switchCase.Body;
      var testValue = (ConstantExpression) switchCase.TestValues.Single();

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Stub (_ => _.IsEvaluatableSwitch (expression)).Repeat.Never();
      filterMock.Stub (_ => _.IsEvaluatableConstant (switchValue)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableSwitchCase (switchCase)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableDefault (caseBody)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableConstant (testValue)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (caseBody), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (testValue), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }


    [Test]
    public void VisitTry_IsEvaluatable_ReturnsTrueForExpression ()
    {
      var expression = (TryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Try);
      var body = (ConstantExpression) expression.Body;
      var finallyBlock = (ConstantExpression) expression.Finally;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableTry (expression)).Return (true);
      filterMock.Expect (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (finallyBlock)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (body), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.True);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitTry_IsNotEvaluatable_ReturnsFalseForExpression ()
    {
      var expression = (TryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Try);
      var body = (ConstantExpression) expression.Body;
      var finallyBlock = (ConstantExpression) expression.Finally;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableTry (expression)).Return (false);
      filterMock.Expect (_ => _.IsEvaluatableConstant (body)).Return (true);
      filterMock.Stub (_ => _.IsEvaluatableConstant (finallyBlock)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (body), Is.True);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }

    [Test]
    public void VisitTry_WithBodyNotEvaluatable_ReturnsFalseForExpressionAndBody ()
    {
      var expression = (TryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Try);
      var body = (ConstantExpression) expression.Body;
      var finallyBlock = (ConstantExpression) expression.Finally;

      var filterMock = MockRepository.GenerateStrictMock<EvaluatableExpressionFilterBase>();
      filterMock.Expect (_ => _.IsEvaluatableTry (expression)).Repeat.Never();
      filterMock.Expect (_ => _.IsEvaluatableConstant (body)).Return (false);
      filterMock.Stub (_ => _.IsEvaluatableConstant (finallyBlock)).Return (true);
      filterMock.Replay();

      var evaluationInfo = EvaluatableTreeFindingExpressionVisitor.Analyze (expression, filterMock);
      Assert.That (evaluationInfo.IsEvaluatableExpression (body), Is.False);
      Assert.That (evaluationInfo.IsEvaluatableExpression (expression), Is.False);

      filterMock.VerifyAllExpectations();
    }
#endif
  }
}