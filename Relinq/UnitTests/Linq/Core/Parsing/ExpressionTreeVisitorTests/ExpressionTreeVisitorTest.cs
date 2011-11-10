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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using System.Linq;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests
{
  [TestFixture]
  public class ExpressionTreeVisitorTest : ExpressionTreeVisitorTestBase
  {
    [Test]
    public void IsSupportedStandardExpression_True ()
    {
      var supportedExpressionTypeValues = 
          new[]
          {
              ExpressionType.ArrayLength, ExpressionType.Convert, ExpressionType.ConvertChecked, ExpressionType.Negate, ExpressionType.NegateChecked,
              ExpressionType.Not, ExpressionType.Quote, ExpressionType.TypeAs, ExpressionType.UnaryPlus, ExpressionType.Add, ExpressionType.AddChecked,
              ExpressionType.Divide, ExpressionType.Modulo, ExpressionType.Multiply, ExpressionType.MultiplyChecked, ExpressionType.Power,
              ExpressionType.Subtract, ExpressionType.SubtractChecked, ExpressionType.And, ExpressionType.Or, ExpressionType.ExclusiveOr,
              ExpressionType.LeftShift, ExpressionType.RightShift, ExpressionType.AndAlso, ExpressionType.OrElse, ExpressionType.Equal,
              ExpressionType.NotEqual, ExpressionType.GreaterThanOrEqual, ExpressionType.GreaterThan, ExpressionType.LessThan,
              ExpressionType.LessThanOrEqual, ExpressionType.Coalesce, ExpressionType.ArrayIndex, ExpressionType.Conditional, ExpressionType.Constant,
              ExpressionType.Invoke, ExpressionType.Lambda, ExpressionType.MemberAccess, ExpressionType.Call, ExpressionType.New,
              ExpressionType.NewArrayBounds, ExpressionType.NewArrayInit, ExpressionType.MemberInit, ExpressionType.ListInit, ExpressionType.Parameter, 
              ExpressionType.TypeIs,
          };

      var visitMethodExpressionTypes = new HashSet<Type> (
          from m in typeof (ExpressionTreeVisitor).GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
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
        Assert.That (
            visitMethodExpressionTypes.Any (t => t.IsAssignableFrom (expressionInstance.GetType ())), 
            Is.True, 
            "Visit method for {0}", 
            expressionInstance.GetType ());
        Assert.That (ExpressionTreeVisitor.IsSupportedStandardExpression (expressionInstance), Is.True);
      }
    }

    [Test]
    public void IsSupportedStandardExpression_False ()
    {
      var extensionExpression = new TestExtensionExpression (Expression.Constant (0));
      Assert.That (ExpressionTreeVisitor.IsSupportedStandardExpression (extensionExpression), Is.False);

      var unknownExpression = new UnknownExpression (typeof (int));
      Assert.That (ExpressionTreeVisitor.IsSupportedStandardExpression (unknownExpression), Is.False);

      var querySourceReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int());
      Assert.That (ExpressionTreeVisitor.IsSupportedStandardExpression (querySourceReferenceExpression), Is.False);

      var subQueryExpression = new SubQueryExpression (ExpressionHelper.CreateQueryModel_Cook());
      Assert.That (ExpressionTreeVisitor.IsSupportedStandardExpression (subQueryExpression), Is.False);
    }

    [Test]
    public void IsRelinqExpression ()
    {
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ());
      Assert.That (ExpressionTreeVisitor.IsRelinqExpression (querySourceReferenceExpression), Is.True);

      var subQueryExpression = new SubQueryExpression (ExpressionHelper.CreateQueryModel_Cook ());
      Assert.That (ExpressionTreeVisitor.IsRelinqExpression (subQueryExpression), Is.True);

      var standardExpression = Expression.Constant (0);
      Assert.That (ExpressionTreeVisitor.IsRelinqExpression (standardExpression), Is.False);
      
      var extensionExpression = new TestExtensionExpression (Expression.Constant (0));
      Assert.That (ExpressionTreeVisitor.IsRelinqExpression (extensionExpression), Is.False);

      var unknownExpression = new UnknownExpression (typeof (int));
      Assert.That (ExpressionTreeVisitor.IsRelinqExpression (unknownExpression), Is.False);
    }

    [Test]
    public void IsExtensionExpression ()
    {
      var extensionExpression = new TestExtensionExpression (Expression.Constant (0));
      Assert.That (ExpressionTreeVisitor.IsExtensionExpression (extensionExpression), Is.True);

      var standardExpression = Expression.Constant (0);
      Assert.That (ExpressionTreeVisitor.IsExtensionExpression (standardExpression), Is.False);

      var unknownExpression = new UnknownExpression (typeof (int));
      Assert.That (ExpressionTreeVisitor.IsExtensionExpression (unknownExpression), Is.False);

      var querySourceReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ());
      Assert.That (ExpressionTreeVisitor.IsExtensionExpression (querySourceReferenceExpression), Is.False);

      var subQueryExpression = new SubQueryExpression (ExpressionHelper.CreateQueryModel_Cook ());
      Assert.That (ExpressionTreeVisitor.IsExtensionExpression (subQueryExpression), Is.False);
    }

    [Test]
    public void IsUnknownNonExtensionExpression ()
    {
      var unknownExpression = new UnknownExpression (typeof (int));
      Assert.That (ExpressionTreeVisitor.IsUnknownNonExtensionExpression (unknownExpression), Is.True);

      var standardExpression = Expression.Constant (0);
      Assert.That (ExpressionTreeVisitor.IsUnknownNonExtensionExpression (standardExpression), Is.False);

      var extensionExpression = new TestExtensionExpression (Expression.Constant (0));
      Assert.That (ExpressionTreeVisitor.IsUnknownNonExtensionExpression (extensionExpression), Is.False);

      var querySourceReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ());
      Assert.That (ExpressionTreeVisitor.IsUnknownNonExtensionExpression (querySourceReferenceExpression), Is.False);

      var subQueryExpression = new SubQueryExpression (ExpressionHelper.CreateQueryModel_Cook ());
      Assert.That (ExpressionTreeVisitor.IsUnknownNonExtensionExpression (subQueryExpression), Is.False);
    }

    [Test]
    public void AdjustArgumentsForNewExpression ()
    {
      var arguments = new[] { Expression.Constant (0), Expression.Constant ("string1"), Expression.Constant ("string2") };
      var tupleType = typeof (Tuple<double, object, string>);
      var members = new MemberInfo[] { tupleType.GetProperty ("Item1"), tupleType.GetMethod ("get_Item2"), tupleType.GetProperty ("Item3") };

      var result = ExpressionTreeVisitor.AdjustArgumentsForNewExpression (arguments, members).ToArray();

      Assert.That (result.Length, Is.EqualTo (3));
      var expected1 = Expression.Convert (arguments[0], typeof (double));
      ExpressionTreeComparer.CheckAreEqualTrees (expected1, result[0]);
      var expected2 = Expression.Convert (arguments[1], typeof (object));
      ExpressionTreeComparer.CheckAreEqualTrees (expected2, result[1]);
      Assert.That (result[2], Is.SameAs (arguments[2]));
    }

    [Test]
    public void VisitExpression_ExtensionExpression ()
    {
      var expectedResult = Expression.Constant (0);

      var visitor = new TestableExpressionTreeVisitor();

      var extensionExpressionMock = MockRepository.StrictMock<ExtensionExpression> (typeof (int));
      extensionExpressionMock.Expect (mock => mock.Accept (visitor)).Return (expectedResult);
      extensionExpressionMock.Replay();

      var result = visitor.VisitExpression (extensionExpressionMock);
      extensionExpressionMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void VisitExpression_Unknown ()
    {
      CheckDelegation ("VisitUnknownNonExtensionExpression", (ExpressionType) (-1));
    }

    [Test]
    public void VisitExpression_Null ()
    {
      var visitor = MockRepository.PartialMock<ExpressionTreeVisitor>();
      MockRepository.ReplayAll();
      Assert.IsNull (visitor.VisitExpression (null));
    }

    [Test]
    public void VisitExpression_Binary ()
    {
      CheckDelegation (
          "VisitBinaryExpression",
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
    public void VisitExpression_Conditional ()
    {
      CheckDelegation ("VisitConditionalExpression", ExpressionType.Conditional);
    }

    [Test]
    public void VisitExpression_Constant ()
    {
      CheckDelegation ("VisitConstantExpression", ExpressionType.Constant);
    }

    [Test]
    public void VisitExpression_Invoke ()
    {
      CheckDelegation ("VisitInvocationExpression", ExpressionType.Invoke);
    }

    [Test]
    public void VisitExpression_Lambda ()
    {
      CheckDelegation ("VisitLambdaExpression", ExpressionType.Lambda);
    }

    [Test]
    public void VisitExpression_Member ()
    {
      CheckDelegation ("VisitMemberExpression", ExpressionType.MemberAccess);
    }

    [Test]
    public void VisitExpression_MethodCall ()
    {
      CheckDelegation ("VisitMethodCallExpression", ExpressionType.Call);
    }

    [Test]
    public void VisitExpression_New ()
    {
      CheckDelegation ("VisitNewExpression", ExpressionType.New);
    }

    [Test]
    public void VisitExpression_NewAray ()
    {
      CheckDelegation ("VisitNewArrayExpression", ExpressionType.NewArrayBounds, ExpressionType.NewArrayInit);
    }

    [Test]
    public void VisitExpression_MemberInit ()
    {
      CheckDelegation ("VisitMemberInitExpression", ExpressionType.MemberInit);
    }

    [Test]
    public void VisitExpression_ListInit ()
    {
      CheckDelegation ("VisitListInitExpression", ExpressionType.ListInit);
    }

    [Test]
    public void VisitExpression_Parameter ()
    {
      CheckDelegation ("VisitParameterExpression", ExpressionType.Parameter);
    }

    [Test]
    public void VisitExpression_TypeBinary ()
    {
      CheckDelegation ("VisitTypeBinaryExpression", ExpressionType.TypeIs);
    }

    [Test]
    public void VisitExpression_Unary ()
    {
      CheckDelegation (
          "VisitUnaryExpression",
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
    public void VisitExpression_SubQuery ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel_Cook();
      CheckDelegation ("VisitSubQueryExpression", new SubQueryExpression (queryModel));
    }

    [Test]
    public void VisitExpression_QuerySourceReference ()
    {
      var clause = ExpressionHelper.CreateMainFromClause_Int();
      CheckDelegation ("VisitQuerySourceReferenceExpression", new QuerySourceReferenceExpression (clause));
    }

    [Test]
    public void VisitAndConvert_Single_OriginalNull ()
    {
      var result = InvokeAndCheckVisitAndConvertExpression<Expression> (null, "Add");
      Assert.That (result, Is.Null);
    }

    [Test]
    public void VisitAndConvert_Single_NoConvert ()
    {
      var expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      Expect.Call (VisitorMock.VisitExpression (expression)).Return (expression);

      var result = InvokeAndCheckVisitAndConvertExpression (expression, "Add");

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitAndConvert_Single_NewExpression ()
    {
      var expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      var newExpression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Or);

      Expect.Call (VisitorMock.VisitExpression (expression)).Return (newExpression);

      var result = InvokeAndCheckVisitAndConvertExpression (expression, "Add");

      Assert.That (result, Is.SameAs (newExpression));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "When called from 'VisitMethod', expressions of type 'BinaryExpression' can only be replaced with other non-null expressions of type "
        + "'BinaryExpression'.")
    ]
    public void VisitAndConvert_Single_ThrowsOnInvalidType ()
    {
      var expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      var newExpression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);

      Expect.Call (VisitorMock.VisitExpression (expression)).Return (newExpression);

      InvokeAndCheckVisitAndConvertExpression (expression, "VisitMethod");
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "When called from 'VisitMethod', expressions of type 'BinaryExpression' can only be replaced with other non-null expressions of type "
        + "'BinaryExpression'.")
    ]
    public void VisitAndConvert_Single_ThrowsOnNull ()
    {
      var expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);

      Expect.Call (VisitorMock.VisitExpression (expression)).Return (null);

      InvokeAndCheckVisitAndConvertExpression (expression, "VisitMethod");
    }

    [Test]
    public void VisitList_Unchanged ()
    {
      Expression expr1 = Expression.Constant (1);
      ReadOnlyCollection<Expression> expressions = new List<Expression> (new[] { expr1 }).AsReadOnly ();

      Expect.Call (VisitorMock.VisitExpression (expr1)).Return (expr1);

      var result = VisitorMock.VisitList (expressions, arg => InvokeAndCheckVisitAndConvertExpression (expr1, "VisitAndConvert"));
      
      Assert.That (result, Is.SameAs (expressions));
    }

    [Test]
    public void VisitList_Changed ()
    {
      Expression expr1 = Expression.Constant (1);
      Expression expr2 = Expression.Constant (2);
      ReadOnlyCollection<Expression> expressions = new List<Expression> (new[] { expr1 }).AsReadOnly ();

      Expect.Call (VisitorMock.VisitExpression (expr1)).Return (expr2);
      var result = VisitorMock.VisitList (expressions, arg => InvokeAndCheckVisitAndConvertExpression (expr1, "VisitAndConvert"));
      ReadOnlyCollection<Expression> conditionResult = new List<Expression> (new[] { expr2 }).AsReadOnly ();
      Assert.That (result, Is.EqualTo (conditionResult));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "The current list only supports objects of type 'Expression' as its elements.")]
    public void VisitList_Changed_InvalidType ()
    {
      Expression expr1 = Expression.Constant (1);
      ReadOnlyCollection<Expression> expressions = new List<Expression> (new[] { expr1 }).AsReadOnly ();

      VisitorMock.VisitList (expressions, arg => null);
    }

    [Test]
    [Ignore ("Bug in RhinoMocks currently makes it impossible to run this test.")]
    public void VisitAndConvert_Collection ()
    {
      var expr1 = Expression.Constant (1);
      var expressions = new List<ConstantExpression> (new[] { expr1 }).AsReadOnly ();

      Expect.Call (VisitorMock.VisitExpression (expr1)).Return (expr1);
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

      Expect.Call (VisitorMock.VisitExpression (constantExpression)).Return (newExpression);
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
      var visitorMock = MockRepository.StrictMock<ExpressionTreeVisitor>();

      MethodInfo methodToBeCalled = visitorMock.GetType().GetMethod (methodName, BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.IsNotNull (methodToBeCalled);

      foreach (Expression expression in expressions)
      {
        MockRepository.BackToRecord (visitorMock);
        Expect.Call (visitorMock.VisitExpression (expression)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        Expect.Call (methodToBeCalled.Invoke (visitorMock, new object[] { expression })).Return (expression);

        MockRepository.Replay (visitorMock);

        object result = visitorMock.VisitExpression (expression);
        Assert.AreSame (expression, result);
        MockRepository.Verify (visitorMock);
      }
    }
  }
}