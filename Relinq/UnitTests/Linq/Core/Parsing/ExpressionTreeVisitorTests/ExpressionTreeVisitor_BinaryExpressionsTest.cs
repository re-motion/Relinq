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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests
{
  [TestFixture]
  public class ExpressionTreeVisitor_BinaryExpressionsTest : ExpressionTreeVisitorTestBase
  {
    [Test]
    public void VisitBinaryExpression_Unchanged ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      Expect.Call (VisitorMock.VisitExpression (expression.Left)).Return (expression.Left);
      Expect.Call (VisitorMock.VisitExpression (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.VisitExpression (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisitExpression ("VisitBinaryExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitBinaryExpression_LeftChanged ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Subtract);
      Expression newOperand = Expression.Constant (1);
      Expect.Call (VisitorMock.VisitExpression (expression.Left)).Return (newOperand);
      Expect.Call (VisitorMock.VisitExpression (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.VisitExpression (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisitExpression ("VisitBinaryExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Subtract, result.NodeType);
      Assert.AreSame (newOperand, result.Left);
      Assert.AreSame (expression.Right, result.Right);
    }

    [Test]
    public void VisitBinaryExpression_RightChanged ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Subtract);
      Expression newOperand = Expression.Constant (1);
      Expect.Call (VisitorMock.VisitExpression (expression.Left)).Return (expression.Left);
      Expect.Call (VisitorMock.VisitExpression (expression.Right)).Return (newOperand);
      Expect.Call (VisitorMock.VisitExpression (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisitExpression ("VisitBinaryExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Subtract, result.NodeType);
      Assert.AreSame (expression.Left, result.Left);
      Assert.AreSame (newOperand, result.Right);
    }

    [Test]
    public void VisitBinaryExpression_ConversionExpression_Unchanged ()
    {
      ParameterExpression conversionParameter = Expression.Parameter (typeof (string), "s");
      LambdaExpression conversion = Expression.Lambda (conversionParameter, conversionParameter);
      BinaryExpression expression = Expression.MakeBinary (
          ExpressionType.Coalesce,
          Expression.Constant ("0"),
          Expression.Constant ("0"),
          false,
          null,
          conversion);

      Expect.Call (VisitorMock.VisitExpression (expression.Left)).Return (expression.Left);
      Expect.Call (VisitorMock.VisitExpression (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.VisitExpression (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisitExpression ("VisitBinaryExpression", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitBinaryExpression_ConversionExpression_Changed ()
    {
      ParameterExpression conversionParameter = Expression.Parameter (typeof (string), "s");
      LambdaExpression conversion = Expression.Lambda (conversionParameter, conversionParameter);
      BinaryExpression expression = Expression.MakeBinary (
          ExpressionType.Coalesce,
          Expression.Constant ("0"),
          Expression.Constant ("0"),
          false,
          null,
          conversion);

      LambdaExpression newConversion = Expression.Lambda (conversionParameter, conversionParameter);

      Expect.Call (VisitorMock.VisitExpression (expression.Left)).Return (expression.Left);
      Expect.Call (VisitorMock.VisitExpression (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.VisitExpression (expression.Conversion)).Return (newConversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisitExpression ("VisitBinaryExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.Conversion, Is.SameAs (newConversion));
    }

    [Test]
    public void VisitBinaryExpression_RespectsIsLiftedToNull ()
    {
      MethodInfo method = ((Func<int, int, bool>) ((i1, i2) => i1 > i2)).Method;

      Expression left = Expression.Constant (0, typeof (int?));
      Expression right = Expression.Constant (0, typeof (int?));

      BinaryExpression expression = Expression.MakeBinary (ExpressionType.GreaterThan, left, right, true, method);

      Expression newOperand = Expression.Constant (1, typeof (int?));
      Expect.Call (VisitorMock.VisitExpression (expression.Left)).Return (newOperand);
      Expect.Call (VisitorMock.VisitExpression (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.VisitExpression (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisitExpression ("VisitBinaryExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.That (result.IsLiftedToNull, Is.True);
    }

    [Test]
    public void VisitBinaryExpression_RespectsMethod ()
    {
      MethodInfo method = ((Func<int, int, bool>) ((i1, i2) => i1 > i2)).Method;

      Expression left = Expression.Constant (0, typeof (int?));
      Expression right = Expression.Constant (0, typeof (int?));

      BinaryExpression expression = Expression.MakeBinary (ExpressionType.GreaterThan, left, right, true, method);

      Expression newOperand = Expression.Constant (1, typeof (int?));
      Expect.Call (VisitorMock.VisitExpression (expression.Left)).Return (newOperand);
      Expect.Call (VisitorMock.VisitExpression (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.VisitExpression (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisitExpression ("VisitBinaryExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.That (result.Method, Is.SameAs (method));
    }
  }
}
