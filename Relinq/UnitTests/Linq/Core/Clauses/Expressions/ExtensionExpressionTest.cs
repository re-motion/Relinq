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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions.TestDomain;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Rhino.Mocks;
using Remotion.Linq.UnitTests.Linq.Core.TestUtilities;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions
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
    public void Accept_CallsVisitExtensionExpression ()
    {
      var visitorMock = _mockRepository.StrictMock<ExpressionTreeVisitor>();

      visitorMock
          .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (visitorMock, "VisitExtensionExpression", _expression))
          .Return (_expression);
      visitorMock.Replay ();

      _expression.Accept (visitorMock);

      visitorMock.VerifyAllExpectations ();
    }

    [Test]
    public void Accept_ReturnsResultOf_VisitExtensionExpression ()
    {
      var visitorMock = _mockRepository.StrictMock<ExpressionTreeVisitor> ();
      var expectedResult = Expression.Constant (0);

      visitorMock
          .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (visitorMock, "VisitExtensionExpression", _expression))
          .Return (expectedResult);
      visitorMock.Replay ();

      var result = _expression.Accept (visitorMock);

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
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Reducible nodes must override the Reduce method.")]
    public void Reduce_Reducible_ThrowsIfNotOverridden ()
    {
      var expression = new ReducibleExtensionExpressionNotOverridingReduce (typeof (int));
      expression.Reduce ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Reduce and check can only be called on reducible nodes.")]   
    public void ReduceAndCheck_ThrowsIfNotReducible ()
    {
      _expression.ReduceAndCheck ();
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
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Reduce cannot return null.")]
    public void ReduceAndCheck_ThrowsIfReducesToNull ()
    {
      var expressionPartialMock = CreateReduciblePartialMock(typeof (int));
      expressionPartialMock.Expect (mock => mock.Reduce ()).Return (null);
      expressionPartialMock.Replay (); 
      
      expressionPartialMock.ReduceAndCheck ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Reduce cannot return the original expression.")]
    public void ReduceAndCheck_ThrowsIfReducesToSame ()
    {
      var expressionPartialMock = CreateReduciblePartialMock (typeof (int));
      expressionPartialMock.Expect (mock => mock.Reduce ()).Return (expressionPartialMock);
      expressionPartialMock.Replay ();

      expressionPartialMock.ReduceAndCheck ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Reduce must produce an expression of a compatible type.")]
    public void ReduceAndCheck_ThrowsIfReducesToDifferentType ()
    {
      var expressionOfDifferentType = Expression.Constant ("string");

      var expressionPartialMock = CreateReduciblePartialMock (typeof (int));
      expressionPartialMock.Expect (mock => mock.Reduce ()).Return (expressionOfDifferentType);
      expressionPartialMock.Replay ();

      expressionPartialMock.ReduceAndCheck ();
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