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
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
#if NET_3_5
using Remotion.Linq.Clauses.ExpressionVisitors;
#endif
using Remotion.Linq.Parsing;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses.Expressions
{
  [TestFixture]
  public class VBStringComparisonExpressionTest
  {
    private VBStringComparisonExpression _expression;
    private BinaryExpression _comparisonExpression;

    [SetUp]
    public void SetUp ()
    {
      _comparisonExpression = Expression.Equal (Expression.Constant ("string1"), Expression.Constant ("string2"));
      _expression = new VBStringComparisonExpression (_comparisonExpression, true);
    }

#if NET_3_5
    [Test]
    public void NodeType ()
    {
      Assert.That (VBStringComparisonExpression.ExpressionType, Is.EqualTo ((ExpressionType) 100003));
      ExtensionExpressionTestHelper.CheckUniqueNodeType (typeof (VBStringComparisonExpression), VBStringComparisonExpression.ExpressionType);
    }
#endif

    [Test]
    public void Initialization ()
    {
#if !NET_3_5
      Assert.That (_expression.NodeType, Is.EqualTo (ExpressionType.Extension));
#else
      Assert.That (_expression.NodeType, Is.EqualTo (VBStringComparisonExpression.ExpressionType));
#endif
    }

    [Test]
    public void Initialization_TypeComesFromComparison ()
    {
      var boolExpression = new VBStringComparisonExpression (Expression.Equal (Expression.Constant ("string1"), Expression.Constant ("string2")), true);
      var intExpression = new VBStringComparisonExpression (Expression.Constant (0), true);

      Assert.That (boolExpression.Type, Is.SameAs (typeof (bool)));
      Assert.That (intExpression.Type, Is.SameAs (typeof (int)));
    }

    [Test]
    public void CanReduce ()
    {
      Assert.That (_expression.CanReduce, Is.True);
    }

    [Test]
    public void Reduce ()
    {
      var result = _expression.Reduce();

      Assert.That (result, Is.SameAs (_comparisonExpression));
    }

    [Test]
    public void VisitChildren_ReturnsSameExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<RelinqExpressionVisitor> ();

      visitorMock
          .Expect (mock => mock.Visit (_comparisonExpression))
          .Return (_comparisonExpression);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_expression));
    }

    [Test]
    public void VisitChildren_ReturnsNewExpression ()
    {
      var newExpression = Expression.Equal (Expression.Constant ("string1"), Expression.Constant ("string"));
      var visitorMock = MockRepository.GenerateStrictMock<RelinqExpressionVisitor> ();

      visitorMock
          .Expect (mock => mock.Visit (_comparisonExpression))
          .Return (newExpression);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_expression));
      Assert.That (((VBStringComparisonExpression) result).Comparison, Is.SameAs (newExpression));
      Assert.That (((VBStringComparisonExpression) result).TextCompare, Is.True);
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<VBStringComparisonExpression, IVBSpecificExpressionVisitor> (
          _expression,
          mock => mock.VisitVBStringComparison (_expression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_expression);
    }

    [Test]
    public void ToString_Directly ()
    {
      var result = _expression.ToString();

#if !NET_3_5
      Assert.That (result, Is.EqualTo ("VBCompareString((\"string1\" == \"string2\"), True)"));
#else
      Assert.That (result, Is.EqualTo ("VBCompareString((\"string1\" = \"string2\"), True)"));
#endif
    }

    [Test]
    public void ToString_Nested ()
    {
      var expression = Expression.Not (_expression);

#if !NET_3_5
      var result = expression.ToString();
      Assert.That (result, Is.EqualTo ("Not(VBCompareString((\"string1\" == \"string2\"), True))"));
#else
      var result = FormattingExpressionVisitor.Format (expression);
      Assert.That (result, Is.EqualTo ("Not(VBCompareString((\"string1\" = \"string2\"), True))"));
#endif
    }

  }
}