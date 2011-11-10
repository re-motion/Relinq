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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions
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

    [Test]
    public void NodeType ()
    {
      Assert.That (VBStringComparisonExpression.ExpressionType, Is.EqualTo ((ExpressionType) 100003));
      ExtensionExpressionTestHelper.CheckUniqueNodeType (typeof (VBStringComparisonExpression), VBStringComparisonExpression.ExpressionType);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_expression.NodeType, Is.EqualTo (VBStringComparisonExpression.ExpressionType));
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
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.VisitExpression (_comparisonExpression))
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
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.VisitExpression (_comparisonExpression))
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
          mock => mock.VisitVBStringComparisonExpression (_expression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_expression);
    }

    [Test]
    public void To_String ()
    {
      var result = _expression.ToString();

      Assert.That (result, Is.EqualTo ("VBCompareString((\"string1\" = \"string2\"), True)"));
    }

  }
}