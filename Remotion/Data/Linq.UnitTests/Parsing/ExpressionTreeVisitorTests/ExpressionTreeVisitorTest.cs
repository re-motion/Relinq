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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.Data.Linq.UnitTests.Parsing.ExpressionTreeVisitorTests
{
  [TestFixture]
  public class ExpressionTreeVisitorTest : ExpressionTreeVisitorTestBase
  {
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
      CheckDelegation ("VisitUnknownExpression", (ExpressionType) (-1));
    }


    [Test]
    public void VisitExpression_Null ()
    {
      ExpressionTreeVisitor visitor = MockRepository.PartialMock<ExpressionTreeVisitor>();
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
    public void VisitAndConvert_NoConvert ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      Expect.Call (VisitorMock.VisitExpression (expression)).Return (expression);

      var result = InvokeAndCheckVisitAndConvertExpression (expression, "Add");

      Assert.That (expression, Is.EqualTo (result));
    }

    [Test]
    public void VisitAndConvert_NewExpression ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      BinaryExpression newExpression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Or);

      Expect.Call (VisitorMock.VisitExpression (expression)).Return (newExpression);

      var result = InvokeAndCheckVisitAndConvertExpression (expression, "Add");

      Assert.That (newExpression, Is.EqualTo (result));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException),
        ExpectedMessage =
            "When called from 'VisitMethod', expressions of type 'NewExpression' can only be replaced with other expressions of type 'BinaryExpression'.")
    ]
    public void VisitAndConvert_ThrowException ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      NewExpression newExpression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);

      Expect.Call (VisitorMock.VisitExpression (expression)).Return (newExpression);

      try
      {
        InvokeAndCheckVisitAndConvertExpression (expression, "VisitMethod");
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException;
      }
    }

    [Test]
    public void VisitExpressionList_Unchanged ()
    {
      Expression expr1 = Expression.Constant (1);
      ReadOnlyCollection<Expression> expressions = new List<Expression> (new[] { expr1 }).AsReadOnly ();

      Expect.Call (VisitorMock.VisitExpression (expr1)).Return (expr1);
      var result = VisitorMock.VisitList (expressions, arg => InvokeAndCheckVisitAndConvertExpression (expr1, "VisitExpressionList"));
      Assert.That (result, Is.SameAs (expressions));
    }

    [Test]
    public void VisitList_Changed ()
    {
      Expression expr1 = Expression.Constant (1);
      Expression expr2 = Expression.Constant (2);
      ReadOnlyCollection<Expression> expressions = new List<Expression> (new[] { expr1 }).AsReadOnly ();

      Expect.Call (VisitorMock.VisitExpression (expr1)).Return (expr2);
      var result = VisitorMock.VisitList (expressions, arg => InvokeAndCheckVisitAndConvertExpression (expr1, "VisitExpressionList"));
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

    private void CheckDelegation (string methodName, params ExpressionType[] expressionTypes)
    {
      Expression[] expressions = Array.ConvertAll<ExpressionType, Expression> (expressionTypes, ExpressionInstanceCreator.GetExpressionInstance);

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