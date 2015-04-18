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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitorTests
{
  [TestFixture]
  public class ExpressionVisitor_BinaryExpressionsTest : ExpressionVisitorTestBase
  {
    [Test]
    public void VisitBinary_Unchanged ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      Expect.Call (VisitorMock.Visit (expression.Left)).Return (expression.Left);
      Expect.Call (VisitorMock.Visit (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.Visit (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisit ("VisitBinary", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitBinary_LeftChanged ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Subtract);
      Expression newOperand = Expression.Constant (1);
      Expect.Call (VisitorMock.Visit (expression.Left)).Return (newOperand);
      Expect.Call (VisitorMock.Visit (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.Visit (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisit ("VisitBinary", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Subtract));
      Assert.That (result.Left, Is.SameAs (newOperand));
      Assert.That (result.Right, Is.SameAs (expression.Right));
    }

    [Test]
    public void VisitBinary_RightChanged ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Subtract);
      Expression newOperand = Expression.Constant (1);
      Expect.Call (VisitorMock.Visit (expression.Left)).Return (expression.Left);
      Expect.Call (VisitorMock.Visit (expression.Right)).Return (newOperand);
      Expect.Call (VisitorMock.Visit (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisit ("VisitBinary", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Subtract));
      Assert.That (result.Left, Is.SameAs (expression.Left));
      Assert.That (result.Right, Is.SameAs (newOperand));
    }

    [Test]
    public void VisitBinary_ConversionExpression_Unchanged ()
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

      Expect.Call (VisitorMock.Visit (expression.Left)).Return (expression.Left);
      Expect.Call (VisitorMock.Visit (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.Visit (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisit ("VisitBinary", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitBinary_ConversionExpression_Changed ()
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

      Expect.Call (VisitorMock.Visit (expression.Left)).Return (expression.Left);
      Expect.Call (VisitorMock.Visit (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.Visit (expression.Conversion)).Return (newConversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisit ("VisitBinary", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.Conversion, Is.SameAs (newConversion));
    }

    [Test]
    public void VisitBinary_RespectsIsLiftedToNull ()
    {
      MethodInfo method = ((Func<int, int, bool>) ((i1, i2) => i1 > i2)).Method;

      Expression left = Expression.Constant (0, typeof (int?));
      Expression right = Expression.Constant (0, typeof (int?));

      BinaryExpression expression = Expression.MakeBinary (ExpressionType.GreaterThan, left, right, true, method);

      Expression newOperand = Expression.Constant (1, typeof (int?));
      Expect.Call (VisitorMock.Visit (expression.Left)).Return (newOperand);
      Expect.Call (VisitorMock.Visit (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.Visit (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisit ("VisitBinary", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.IsLiftedToNull, Is.True);
    }

    [Test]
    public void VisitBinary_RespectsMethod ()
    {
      MethodInfo method = ((Func<int, int, bool>) ((i1, i2) => i1 > i2)).Method;

      Expression left = Expression.Constant (0, typeof (int?));
      Expression right = Expression.Constant (0, typeof (int?));

      BinaryExpression expression = Expression.MakeBinary (ExpressionType.GreaterThan, left, right, true, method);

      Expression newOperand = Expression.Constant (1, typeof (int?));
      Expect.Call (VisitorMock.Visit (expression.Left)).Return (newOperand);
      Expect.Call (VisitorMock.Visit (expression.Right)).Return (expression.Right);
      Expect.Call (VisitorMock.Visit (expression.Conversion)).Return (expression.Conversion);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisit ("VisitBinary", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.Method, Is.SameAs (method));
    }
  }
}
