using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq.Expressions;
using Remotion.Data.Linq.Expressions;
using Rhino.Mocks;
using System.Reflection;
using Rhino.Mocks.Interfaces;
using Remotion.Data.Linq.Visitor;
using Remotion.Development.UnitTesting;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.Data.Linq.UnitTests.VisitorTest.ExpressionTreeVisitorTest
{
  [TestFixture]
  public class ExpressionVisitorTest
  {
    private MockRepository _mockRepository;

    [SetUp]
    public void Setup()
    {
      _mockRepository = new MockRepository();
    }

    [Test]
    public void VisitExpression_Unknown ()
    {
      CheckDelegation (_mockRepository, "VisitUnknownExpression", (ExpressionType) (-1));
    }

    [Test]
    public void VisitExpression_Null ()
    {
      ExpressionTreeVisitor visitor = _mockRepository.PartialMock<ExpressionTreeVisitor>();
      _mockRepository.ReplayAll ();
      Assert.IsNull (PrivateInvoke.InvokeNonPublicMethod (visitor, "VisitExpression", new object[] { null }));
    }

    [Test]
    public void VisitExpression_Binary ()
    {
      CheckDelegation (_mockRepository, "VisitBinaryExpression", ExpressionType.Add, ExpressionType.AddChecked, ExpressionType.Divide,
          ExpressionType.Modulo, ExpressionType.Multiply, ExpressionType.MultiplyChecked, ExpressionType.Power, ExpressionType.Subtract,
          ExpressionType.SubtractChecked, ExpressionType.And, ExpressionType.Or, ExpressionType.ExclusiveOr, ExpressionType.LeftShift,
          ExpressionType.RightShift, ExpressionType.AndAlso, ExpressionType.OrElse, ExpressionType.Equal, ExpressionType.NotEqual,
          ExpressionType.GreaterThanOrEqual, ExpressionType.GreaterThan, ExpressionType.LessThan, ExpressionType.LessThanOrEqual,
          ExpressionType.Coalesce, ExpressionType.ArrayIndex);
    }

    [Test]
    public void VisitExpression_Conditional ()
    {
      CheckDelegation (_mockRepository, "VisitConditionalExpression", ExpressionType.Conditional);
    }

    [Test]
    public void VisitExpression_Constant ()
    {
      CheckDelegation (_mockRepository, "VisitConstantExpression", ExpressionType.Constant);
    }

    [Test]
    public void VisitExpression_Invoke ()
    {
      CheckDelegation (_mockRepository, "VisitInvocationExpression", ExpressionType.Invoke);
    }

    [Test]
    public void VisitExpression_Lambda ()
    {
      CheckDelegation (_mockRepository, "VisitLambdaExpression", ExpressionType.Lambda);
    }

    [Test]
    public void VisitExpression_Member ()
    {
      CheckDelegation (_mockRepository, "VisitMemberExpression", ExpressionType.MemberAccess);
    }

    [Test]
    public void VisitExpression_MethodCall ()
    {
      CheckDelegation (_mockRepository, "VisitMethodCallExpression", ExpressionType.Call);
    }

    [Test]
    public void VisitExpression_New ()
    {
      CheckDelegation (_mockRepository, "VisitNewExpression", ExpressionType.New);
    }

    [Test]
    public void VisitExpression_NewAray ()
    {
      CheckDelegation (_mockRepository, "VisitNewArrayExpression", ExpressionType.NewArrayBounds, ExpressionType.NewArrayInit);
    }

    [Test]
    public void VisitExpression_MemberInit ()
    {
      CheckDelegation (_mockRepository, "VisitMemberInitExpression", ExpressionType.MemberInit);
    }

    [Test]
    public void VisitExpression_ListInit ()
    {
      CheckDelegation (_mockRepository, "VisitListInitExpression", ExpressionType.ListInit);
    }

    [Test]
    public void VisitExpression_Parameter ()
    {
      CheckDelegation (_mockRepository, "VisitParameterExpression", ExpressionType.Parameter);
    }

    [Test]
    public void VisitExpression_TypeBinary ()
    {
      CheckDelegation (_mockRepository, "VisitTypeBinaryExpression", ExpressionType.TypeIs);
    }

    [Test]
    public void VisitExpression_Unary ()
    {
      CheckDelegation (_mockRepository, "VisitUnaryExpression", ExpressionType.UnaryPlus, ExpressionType.Negate, ExpressionType.NegateChecked,
          ExpressionType.Not, ExpressionType.Convert, ExpressionType.ConvertChecked, ExpressionType.ArrayLength, ExpressionType.Quote,
          ExpressionType.TypeAs);
    }

    [Test]
    public void VisitExpression_SubQuery ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      CheckDelegation (_mockRepository, "VisitSubQueryExpression", new SubQueryExpression (queryModel));
    }
    
    private void CheckDelegation (MockRepository repository, string methodName, params ExpressionType[] expressionTypes)
    {
      Expression[] expressions = Array.ConvertAll<ExpressionType, Expression> (expressionTypes, ExpressionInstanceCreator.GetExpressionInstance);

      CheckDelegation(repository, methodName, expressions);
    }

    private void CheckDelegation (MockRepository repository, string methodName, params Expression[] expressions)
    {
      ExpressionTreeVisitor visitorMock = repository.CreateMock<ExpressionTreeVisitor> ();

      MethodInfo visitExpressionMethod = visitorMock.GetType ().GetMethod ("VisitExpression", BindingFlags.NonPublic | BindingFlags.Instance);
      MethodInfo methodToBeCalled = visitorMock.GetType ().GetMethod (methodName, BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.IsNotNull (methodToBeCalled);

      foreach (Expression expression in expressions)
      {
        repository.BackToRecord (visitorMock);
        Expect.Call (visitExpressionMethod.Invoke (visitorMock, new object[] { expression })).CallOriginalMethod (OriginalCallOptions.CreateExpectation);
        Expect.Call (methodToBeCalled.Invoke (visitorMock, new object[] { expression })).Return (expression);
        
        repository.Replay (visitorMock);

        object result = visitExpressionMethod.Invoke (visitorMock, new object[] { expression });
        Assert.AreSame (expression, result);
        repository.Verify (visitorMock);
      }
    }
  }
}