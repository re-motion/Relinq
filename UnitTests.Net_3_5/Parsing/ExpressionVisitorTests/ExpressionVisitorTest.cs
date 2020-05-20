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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Parsing;
using Remotion.Linq.Parsing;
using Remotion.Linq.UnitTests.Clauses.Expressions;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitorTests
{
  [TestFixture]
  public class ExpressionVisitorTest : ExpressionVisitorTestBase
  {
    [Test]
    public void Visit_Extension ()
    {
      var expectedResult = Expression.Constant (0);

      var visitor = new TestableExpressionVisitor();

      var extensionExpressionMock = MockRepository.StrictMock<ExtensionExpression> (typeof (int));
      extensionExpressionMock.Expect (mock => ExtensionExpressionTestHelper.CallAccept (mock, visitor)).Return (expectedResult);
      extensionExpressionMock.Replay();

      var result = visitor.Visit (extensionExpressionMock);
      extensionExpressionMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (expectedResult));
    }

#if NET_3_5
    [Test]
    public void Visit_Unknown ()
    {
      CheckDelegation ("VisitUnknownNonExtension", (ExpressionType) (-1));
    }
#endif

    [Test]
    public void Visit_Null ()
    {
      var visitor = MockRepository.PartialMock<RelinqExpressionVisitor>();
      MockRepository.ReplayAll();
      Assert.That (visitor.Visit ((Expression) null), Is.Null);
    }

    [Test]
    public void Visit_Binary ()
    {
      CheckDelegation (
          "VisitBinary",
          ExpressionType.Add,
          ExpressionType.AddChecked,
          ExpressionType.Divide,
          ExpressionType.Modulo,
          ExpressionType.Multiply,
          ExpressionType.MultiplyChecked,
          ExpressionType.Power,
          ExpressionType.Subtract,
          ExpressionType.SubtractChecked,
          ExpressionType.And,
          ExpressionType.Or,
          ExpressionType.ExclusiveOr,
          ExpressionType.LeftShift,
          ExpressionType.RightShift,
          ExpressionType.AndAlso,
          ExpressionType.OrElse,
          ExpressionType.Equal,
          ExpressionType.NotEqual,
          ExpressionType.GreaterThanOrEqual,
          ExpressionType.GreaterThan,
          ExpressionType.LessThan,
          ExpressionType.LessThanOrEqual,
          ExpressionType.Coalesce,
          ExpressionType.ArrayIndex);
    }

    [Test]
    public void Visit_Conditional ()
    {
      CheckDelegation ("VisitConditional", ExpressionType.Conditional);
    }

    [Test]
    public void Visit_Constant ()
    {
      CheckDelegation ("VisitConstant", ExpressionType.Constant);
    }

    [Test]
    public void Visit_Invoke ()
    {
      CheckDelegation ("VisitInvocation", ExpressionType.Invoke);
    }

    [Test]
    public void Visit_Lambda ()
    {
      CheckDelegation ("VisitLambda", ExpressionType.Lambda);
    }

    [Test]
    public void Visit_Member ()
    {
      CheckDelegation ("VisitMember", ExpressionType.MemberAccess);
    }

    [Test]
    public void Visit_MethodCall ()
    {
      CheckDelegation ("VisitMethodCall", ExpressionType.Call);
    }

    [Test]
    public void Visit_New ()
    {
      CheckDelegation ("VisitNew", ExpressionType.New);
    }

    [Test]
    public void Visit_NewAray ()
    {
      CheckDelegation ("VisitNewArray", ExpressionType.NewArrayBounds, ExpressionType.NewArrayInit);
    }

    [Test]
    public void Visit_MemberInit ()
    {
      CheckDelegation ("VisitMemberInit", ExpressionType.MemberInit);
    }

    [Test]
    public void Visit_ListInit ()
    {
      CheckDelegation ("VisitListInit", ExpressionType.ListInit);
    }

    [Test]
    public void Visit_Parameter ()
    {
      CheckDelegation ("VisitParameter", ExpressionType.Parameter);
    }

    [Test]
    public void Visit_TypeBinary ()
    {
      CheckDelegation ("VisitTypeBinary", ExpressionType.TypeIs);
    }

    [Test]
    public void Visit_Unary ()
    {
      CheckDelegation (
          "VisitUnary",
          ExpressionType.UnaryPlus,
          ExpressionType.Negate,
          ExpressionType.NegateChecked,
          ExpressionType.Not,
          ExpressionType.Convert,
          ExpressionType.ConvertChecked,
          ExpressionType.ArrayLength,
          ExpressionType.Quote,
          ExpressionType.TypeAs);
    }

    [Test]
    public void Visit_SubQuery ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel<Cook>();
      CheckDelegation ("VisitSubQuery", new SubQueryExpression (queryModel));
    }

    [Test]
    public void Visit_QuerySourceReference ()
    {
      var clause = ExpressionHelper.CreateMainFromClause_Int();
      CheckDelegation ("VisitQuerySourceReference", new QuerySourceReferenceExpression (clause));
    }

    [Test]
    public void VisitAndConvert_Single_OriginalNull ()
    {
      var result = InvokeVisitAndConvert<Expression> (null, "Add");
      Assert.That (result, Is.Null);
    }

    [Test]
    public void VisitAndConvert_Single_NoConvert ()
    {
      var expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      Expect.Call (VisitorMock.Visit (expression)).Return (expression);

      var result = InvokeVisitAndConvert (expression, "Add");

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitAndConvert_Single_NewExpression ()
    {
      var expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      var newExpression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Or);

      Expect.Call (VisitorMock.Visit (expression)).Return (newExpression);

      var result = InvokeVisitAndConvert (expression, "Add");

      Assert.That (result, Is.SameAs (newExpression));
    }

    [Test]
#endif
    public void VisitAndConvert_Single_ThrowsOnInvalidType ()
    {
      var expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      var newExpression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);

      Expect.Call (VisitorMock.Visit (expression)).Return (newExpression);
      Assert.That (
          () => InvokeVisitAndConvert (expression, "VisitMethod"),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "When called from 'VisitMethod', expressions of type 'BinaryExpression' can only be replaced with other non-null expressions of type "
                  + "'BinaryExpression'."));
    }

    [Test]
#endif
    public void VisitAndConvert_Single_ThrowsOnNull ()
    {
      var expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);

      Expect.Call (VisitorMock.Visit (expression)).Return (null);
      Assert.That (
          () => InvokeVisitAndConvert (expression, "VisitMethod"),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "When called from 'VisitMethod', expressions of type 'BinaryExpression' can only be replaced with other non-null expressions of type "
                  + "'BinaryExpression'."));
    }

    [Test]
    public void VisitList_Unchanged ()
    {
      Expression expr1 = Expression.Constant (1);
      ReadOnlyCollection<Expression> expressions = new List<Expression> (new[] { expr1 }).AsReadOnly ();

      Expect.Call (VisitorMock.Visit (expr1)).Return (expr1);

      var result = ExpressionVisitor.Visit (expressions, arg => InvokeVisitAndConvert (expr1, "VisitAndConvert"));
      
      Assert.That (result, Is.SameAs (expressions));
    }

    [Test]
    public void VisitList_Changed ()
    {
      Expression expr1 = Expression.Constant (1);
      Expression expr2 = Expression.Constant (2);
      ReadOnlyCollection<Expression> expressions = new List<Expression> (new[] { expr1 }).AsReadOnly ();

      Expect.Call (VisitorMock.Visit (expr1)).Return (expr2);
      var result = ExpressionVisitor.Visit (expressions, arg => InvokeVisitAndConvert (expr1, "VisitAndConvert"));
      ReadOnlyCollection<Expression> conditionResult = new List<Expression> (new[] { expr2 }).AsReadOnly ();
      Assert.That (result, Is.EqualTo (conditionResult));
    }

    [Test]
    public void VisitList_Changed_SupportsNullValues ()
    {
      Expression expr1 = Expression.Constant (1);
      Expression expr2 = Expression.Constant (2);
      ReadOnlyCollection<Expression> expressions = new List<Expression> (new[] { expr1, expr2 }).AsReadOnly ();

      var result = ExpressionVisitor.Visit (expressions, arg => arg == expr1 ? null : arg);

      Assert.That (result, Is.EqualTo (new[] { null, expr2 }));
    }

    [Test]
    [Ignore ("Bug in RhinoMocks currently makes it impossible to run this test.")]
    public void VisitAndConvert_Collection ()
    {
      var expr1 = Expression.Constant (1);
      var expressions = new List<ConstantExpression> (new[] { expr1 }).AsReadOnly ();

      Expect.Call (VisitorMock.Visit (expr1)).Return (expr1);
      VisitorMock.Replay ();

      var result = VisitorMock.VisitAndConvert (expressions, "Whatever");

      Assert.That (result, Is.SameAs (expressions));
      VisitorMock.VerifyAllExpectations ();
    }

    [Test]
    [Ignore ("Bug in RhinoMocks currently makes it impossible to run this test.")]
    public void VisitAndConvert_Collection_ExceptionUsesCallerName ()
    {
      var constantExpression = Expression.Constant (1);
      var expressions = new List<ConstantExpression> (new[] { constantExpression }).AsReadOnly ();

      var newExpression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);

      Expect.Call (VisitorMock.Visit (constantExpression)).Return (newExpression);
      VisitorMock.Replay ();

      VisitorMock.VisitAndConvert (expressions, "Whatever");
    }

    private void CheckDelegation (string methodName, params ExpressionType[] expressionTypes)
    {
      Expression[] expressions = Array.ConvertAll (expressionTypes, ExpressionInstanceCreator.GetExpressionInstance);

      CheckDelegation (methodName, expressions);
    }

    private void CheckDelegation (string methodName, params Expression[] expressions)
    {
      var visitorMock = MockRepository.StrictMock<RelinqExpressionVisitor>();

      MethodInfo methodToBeCalled = visitorMock.GetType().GetMethod (methodName, BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (methodToBeCalled, Is.Not.Null);

      foreach (Expression expression in expressions)
      {
        MockRepository.BackToRecord (visitorMock);
        Expect.Call (visitorMock.Visit (expression)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);

        MethodInfo methodToBeCalledWithoutGenericParameters;
        if (methodToBeCalled.ContainsGenericParameters)
          methodToBeCalledWithoutGenericParameters = methodToBeCalled.MakeGenericMethod (expression.GetType().GetGenericArguments());
        else
          methodToBeCalledWithoutGenericParameters = methodToBeCalled;

        Expect.Call (methodToBeCalledWithoutGenericParameters.Invoke (visitorMock, new object[] { expression })).Return (expression);

        MockRepository.Replay (visitorMock);

        object result = visitorMock.Visit (expression);
        Assert.That (result, Is.SameAs (expression));
        MockRepository.Verify (visitorMock);
      }
    }
  }
}