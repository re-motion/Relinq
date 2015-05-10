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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing;
using Remotion.Linq.UnitTests.Clauses.Expressions.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses.Expressions
{
  [TestFixture]
  public class ExtensionExpressionTest
  {
    private TestableExtensionExpression _expression;
    private MockRepository _mockRepository;

    [SetUp]
    public void SetUp ()
    {
      _expression = new TestableExtensionExpression (typeof (int));
      _mockRepository = new MockRepository ();
    }

    [Test]
    public void NodeType ()
    {
      Assert.That (ExtensionExpression.DefaultExtensionExpressionNodeType, Is.EqualTo ((ExpressionType) 150000));
      ExtensionExpressionTestHelper.CheckUniqueNodeType (typeof (ExtensionExpression), ExtensionExpression.DefaultExtensionExpressionNodeType);
    }

    [Test]
    public void Initialize_WithoutNodeType ()
    {
      Assert.That (_expression.Type, Is.SameAs (typeof (int)));
      Assert.That (_expression.NodeType, Is.EqualTo (ExtensionExpression.DefaultExtensionExpressionNodeType));
    }

    [Test]
    public void Initialize_WithNodeType ()
    {
      var expression = new TestableExtensionExpression (typeof (int), (ExpressionType) 150001);
      Assert.That (expression.Type, Is.SameAs (typeof (int)));
      Assert.That (expression.NodeType, Is.EqualTo ((ExpressionType) 150001));
    }

    [Test]
    public void Accept_CallsVisitExtension ()
    {
      var visitorMock = _mockRepository.StrictMock<RelinqExpressionVisitor>();

      visitorMock
          .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (visitorMock, "VisitExtension", _expression))
          .Return (_expression);
      visitorMock.Replay ();

      ExtensionExpressionTestHelper.CallAccept (_expression, visitorMock);

      visitorMock.VerifyAllExpectations ();
    }

    [Test]
    public void Accept_ReturnsResultOf_VisitExtension ()
    {
      var visitorMock = _mockRepository.StrictMock<RelinqExpressionVisitor> ();
      var expectedResult = Expression.Constant (0);

      visitorMock
          .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (visitorMock, "VisitExtension", _expression))
          .Return (expectedResult);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallAccept (_expression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void CanReduce_False ()
    {
      Assert.That (_expression.CanReduce, Is.False);
    }

    [Test]
    public void Reduce_NotReducible_ReturnsThis ()
    {
      Assert.That (_expression.Reduce (), Is.SameAs (_expression));
    }

    [Test]
    public void Reduce_Reducible_ThrowsIfNotOverridden ()
    {
      var expression = new ReducibleExtensionExpressionNotOverridingReduce (typeof (int));
      Assert.That (
          () => expression.Reduce(),
#if !NET_3_5
          Throws.ArgumentException
#else
          Throws.InvalidOperationException.With.Message.EqualTo ("Reducible nodes must override the Reduce method.")
#endif
          );
    }

    [Test]
    public void ReduceAndCheck_ThrowsIfNotReducible ()
    {
      Assert.That (
          () => _expression.ReduceAndCheck(),
#if !NET_3_5
          Throws.ArgumentException
#else
          Throws.InvalidOperationException.With.Message.EqualTo ("Reduce and check can only be called on reducible nodes.")
#endif
          );
    }

    [Test]
    public void ReduceAndCheck_CallsReduce_IfReducible ()
    {
      var expression = new ReducibleExtensionExpression (typeof (int));

      var result = expression.ReduceAndCheck ();
      
      var expectedResult = Expression.Constant (0);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ReduceAndCheck_ThrowsIfReducesToNull ()
    {
      var expressionPartialMock = CreateReduciblePartialMock(typeof (int));
      expressionPartialMock.Expect (mock => mock.Reduce ()).Return (null);
      expressionPartialMock.Replay ();

      Assert.That (
          () => expressionPartialMock.ReduceAndCheck(),
#if !NET_3_5
          Throws.ArgumentException
#else
          Throws.InvalidOperationException.With.Message.EqualTo ("Reduce cannot return null.")
#endif
          );
    }

    [Test]
    public void ReduceAndCheck_ThrowsIfReducesToSame ()
    {
      var expressionPartialMock = CreateReduciblePartialMock (typeof (int));
      expressionPartialMock.Expect (mock => mock.Reduce ()).Return (expressionPartialMock);
      expressionPartialMock.Replay ();

      Assert.That (
          () => expressionPartialMock.ReduceAndCheck(),
#if !NET_3_5
          Throws.ArgumentException
#else
          Throws.InvalidOperationException.With.Message.EqualTo ("Reduce cannot return the original expression.")
#endif
          );
    }

    [Test]
    public void ReduceAndCheck_ThrowsIfReducesToDifferentType ()
    {
      var expressionOfDifferentType = Expression.Constant ("string");

      var expressionPartialMock = CreateReduciblePartialMock (typeof (int));
      expressionPartialMock.Expect (mock => mock.Reduce ()).Return (expressionOfDifferentType);
      expressionPartialMock.Replay ();

      Assert.That (
          () => expressionPartialMock.ReduceAndCheck(),
#if !NET_3_5
          Throws.ArgumentException
#else
          Throws.InvalidOperationException.With.Message.EqualTo ("Reduce must produce an expression of a compatible type.")
#endif
          );
    }

    [Test]
    public void ReduceAndCheck_SucceedsIfReducesToSubType ()
    {
      var expressionOfSubtype = Expression.Constant (new List<int>());

      var expressionPartialMock = CreateReduciblePartialMock (typeof (IList<int>));
      expressionPartialMock.Expect (mock => mock.Reduce ()).Return (expressionOfSubtype);
      expressionPartialMock.Replay ();

      var result = expressionPartialMock.ReduceAndCheck ();

      Assert.That (result, Is.SameAs (expressionOfSubtype));
    }

    private ExtensionExpression CreateReduciblePartialMock (Type expressionValueType)
    {
      var expressionPartialMock = _mockRepository.PartialMock<ExtensionExpression> (expressionValueType);
      expressionPartialMock.Expect (mock => mock.CanReduce).Return (true);
      return expressionPartialMock;
    }
  }
}