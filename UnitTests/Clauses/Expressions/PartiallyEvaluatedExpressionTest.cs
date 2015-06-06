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
using Remotion.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses.Expressions
{
  [TestFixture]
  public class PartiallyEvaluatedExpressionTest
  {
    private PartiallyEvaluatedExpression _expression;
    private MethodCallExpression _originalExpression;
    private ConstantExpression _evaluatedExpression;

    [SetUp]
    public void SetUp ()
    {
      _originalExpression = Expression.Call (ReflectionUtility.GetMethod (() => Guid.NewGuid()));
      _evaluatedExpression = Expression.Constant (Guid.NewGuid());
      _expression = new PartiallyEvaluatedExpression (_originalExpression, _evaluatedExpression);
    }

    [Test]
    public void NodeType ()
    {
#if !NET_3_5
       Assert.That (_expression.NodeType, Is.EqualTo (ExpressionType.Extension));
#else
      Assert.That (PartiallyEvaluatedExpression.ExpressionType, Is.EqualTo ((ExpressionType) 100005));
      ExtensionExpressionTestHelper.CheckUniqueNodeType (typeof (PartiallyEvaluatedExpression), PartiallyEvaluatedExpression.ExpressionType);
      Assert.That (_expression.NodeType, Is.EqualTo (PartiallyEvaluatedExpression.ExpressionType));
#endif
    }

    [Test]
    public void Initialization_TypeComesFromEvaluatedExpression ()
    {
      var boolExpression = new PartiallyEvaluatedExpression (
          Expression.Equal (Expression.Constant ("string1"), Expression.Constant ("string2")),
          Expression.Constant (false, typeof (bool)));
      var guidExpression = new PartiallyEvaluatedExpression (_originalExpression, _evaluatedExpression);

      Assert.That (boolExpression.Type, Is.SameAs (typeof (bool)));
      Assert.That (guidExpression.Type, Is.SameAs (typeof (Guid)));
    }

    [Test]
    public void Initialization_TypeOfEvaluatedExpressionDoesNotMatchTypeOfOriginalExpression_Throws ()
    {
      var expressionWithConcreteType = Expression.Constant (false, typeof (bool));
      var expressionWithBaseType = Expression.Constant (false, typeof (object));

      Assert.That (() => new PartiallyEvaluatedExpression (expressionWithConcreteType, expressionWithBaseType), 
        Throws.ArgumentException.With.Message.EqualTo ("Type 'System.Boolean' of parameter 'originalExpression' does not match Type 'System.Object' of parameter 'evaluatedExpression'."));

      Assert.That (() => new PartiallyEvaluatedExpression (expressionWithBaseType, expressionWithConcreteType),
        Throws.ArgumentException.With.Message.EqualTo ("Type 'System.Object' of parameter 'originalExpression' does not match Type 'System.Boolean' of parameter 'evaluatedExpression'."));
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

      Assert.That (result, Is.SameAs (_evaluatedExpression));
    }

    [Test]
    public void VisitChildren_ReturnsSameExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<RelinqExpressionVisitor> ();

      visitorMock
          .Expect (mock => mock.Visit (_originalExpression))
          .Return (_originalExpression);

      visitorMock
          .Expect (mock => mock.Visit (_evaluatedExpression))
          .Return (_evaluatedExpression);

      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_expression));
    }

    [Test]
    public void VisitChildren_WithChangedOriginalExpression_ReturnsNewExpression ()
    {
      var newExpression = Expression.Parameter (typeof (Guid), "p1");
      var visitorMock = MockRepository.GenerateStrictMock<RelinqExpressionVisitor> ();

      visitorMock
          .Expect (mock => mock.Visit (_originalExpression))
          .Return (newExpression);

      visitorMock
          .Expect (mock => mock.Visit (_evaluatedExpression))
          .Return (_evaluatedExpression);

      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_expression));
      Assert.That (((PartiallyEvaluatedExpression) result).OriginalExpression, Is.SameAs (newExpression));
      Assert.That (((PartiallyEvaluatedExpression) result).EvaluatedExpression, Is.SameAs (_evaluatedExpression));
    }

    [Test]
    public void VisitChildren_WithChangedEvaluatedExpression_ReturnsNewExpression ()
    {
      var newExpression = Expression.Constant (Guid.NewGuid());
      var visitorMock = MockRepository.GenerateStrictMock<RelinqExpressionVisitor> ();

      visitorMock
          .Expect (mock => mock.Visit (_originalExpression))
          .Return (_originalExpression);

      visitorMock
          .Expect (mock => mock.Visit (_evaluatedExpression))
          .Return (newExpression);

      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_expression));
      Assert.That (((PartiallyEvaluatedExpression) result).OriginalExpression, Is.SameAs (_originalExpression));
      Assert.That (((PartiallyEvaluatedExpression) result).EvaluatedExpression, Is.SameAs (newExpression));
    }

    [Test]
    public void VisitChildren_EvaluatedExpressionIsNotConstant_ThrowsInvalidOperationException ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<RelinqExpressionVisitor>();

      visitorMock
          .Stub (mock => mock.Visit (_originalExpression))
          .Return (_originalExpression);

      visitorMock
          .Stub (mock => mock.Visit (_evaluatedExpression))
          // ReSharper disable once RedundantCast
          .Return ((MethodCallExpression) _originalExpression);

      visitorMock.Replay();

      Assert.That (
          () => ExtensionExpressionTestHelper.CallVisitChildren (_expression, visitorMock),
          Throws.InvalidOperationException.With.Message.StartsWith (
#if !NET_3_5
              "When called from 'VisitChildren', rewriting a node of type 'System.Linq.Expressions.ConstantExpression' must return a non-null value of the same type."
#else
              "When called from 'VisitChildren', expressions of type 'ConstantExpression' can only be replaced with other non-null expressions of type 'ConstantExpression'."
#endif
              ));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<PartiallyEvaluatedExpression, IPartialEvaluationExpressionVisitor> (
          _expression,
          mock => mock.VisitPartiallyEvaluated (_expression));
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

      Assert.That (result, Is.EqualTo ("PartiallyEvaluated(NewGuid(), " + (Guid) _evaluatedExpression.Value + ")"));
    }

    [Test]
    public void ToString_Nested ()
    {
      var expression = Expression.Equal (Expression.Constant (Guid.Empty), _expression);

#if !NET_3_5
      var result = expression.ToString();
      Assert.That (
          result,
          Is.EqualTo ("(00000000-0000-0000-0000-000000000000 == PartiallyEvaluated(NewGuid(), " + (Guid) _evaluatedExpression.Value + "))"));
#else
      var result = FormattingExpressionVisitor.Format (expression);
      Assert.That (
          result,
          Is.EqualTo ("(00000000-0000-0000-0000-000000000000 = PartiallyEvaluated(NewGuid(), " + (Guid) _evaluatedExpression.Value + "))"));
#endif
    }

  }
}