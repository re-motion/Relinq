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
using System;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Development.UnitTesting;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.Data.UnitTests.Linq.Parsing.ExpressionTreeVisitorTests
{
  [TestFixture]
  public class ExpressionTreeVisitorTest
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

    [Test]
    public void VisitExpression_QuerySourceReference ()
    {
      var clause = ExpressionHelper.CreateMainFromClause ();
      CheckDelegation (_mockRepository, "VisitQuerySourceReferenceExpression", new QuerySourceReferenceExpression (clause));
    }
    
    private void CheckDelegation (MockRepository repository, string methodName, params ExpressionType[] expressionTypes)
    {
      Expression[] expressions = Array.ConvertAll<ExpressionType, Expression> (expressionTypes, ExpressionInstanceCreator.GetExpressionInstance);

      CheckDelegation(repository, methodName, expressions);
    }

    private void CheckDelegation (MockRepository repository, string methodName, params Expression[] expressions)
    {
      ExpressionTreeVisitor visitorMock = repository.StrictMock<ExpressionTreeVisitor> ();

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
