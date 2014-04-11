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
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors
{
  [TestFixture]
  public class TransformingExpressionTreeVisitorTest
  {
    [Test]
    public void Transform_SingleMatchingTransformation ()
    {
      var inputExpression = CreateSimpleExpression(0);
      var transformedExpression = CreateSimpleExpression(0);

      var providerMock = MockRepository.GenerateStrictMock<IExpressionTranformationProvider> ();
      providerMock
          .Expect (mock => mock.GetTransformations (inputExpression))
          .Return (new ExpressionTransformation[] { expr => transformedExpression });
      providerMock
          .Expect (mock => mock.GetTransformations (transformedExpression))
          .Return (new ExpressionTransformation[0]);
      providerMock.Replay ();

      var result = TransformingExpressionTreeVisitor.Transform (inputExpression, providerMock);

      providerMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (transformedExpression));
    }

    [Test]
    public void Transform_NoMatchingTransformation ()
    {
      var inputExpression = CreateSimpleExpression(0);

      var providerMock = MockRepository.GenerateStrictMock<IExpressionTranformationProvider> ();
      providerMock
          .Expect (mock => mock.GetTransformations (inputExpression))
          .Return (new ExpressionTransformation[0]);
      providerMock.Replay();

      var result = TransformingExpressionTreeVisitor.Transform (inputExpression, providerMock);

      providerMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (inputExpression));
    }

    [Test]
    public void Transform_TwoMatchingTransformations_FirstTransformationChangesExpression ()
    {
      var inputExpression = CreateSimpleExpression(0);
      var transformedExpression = CreateSimpleExpression(0);

      var providerMock = MockRepository.GenerateStrictMock<IExpressionTranformationProvider> ();
      providerMock
          .Expect (mock => mock.GetTransformations (inputExpression))
          .Return (new ExpressionTransformation[] { 
              expr => transformedExpression, 
              expr => { throw new InvalidOperationException ("Must not be called."); }
          });
      providerMock
          .Expect (mock => mock.GetTransformations (transformedExpression))
          .Return (new ExpressionTransformation[0]);
      providerMock.Replay();

      var result = TransformingExpressionTreeVisitor.Transform (inputExpression, providerMock);

      providerMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (transformedExpression));
    }

    [Test]
    public void Transform_TwoMatchingTransformations_FirstTransformationDoesNotChangeExpression ()
    {
      var inputExpression = CreateSimpleExpression(0);
      var transformedExpression = CreateSimpleExpression(0);

      var providerMock = MockRepository.GenerateStrictMock<IExpressionTranformationProvider> ();
      providerMock
          .Expect (mock => mock.GetTransformations (inputExpression))
          .Return (new ExpressionTransformation[] { 
              expr => expr, 
              expr => transformedExpression
          });
      providerMock
          .Expect (mock => mock.GetTransformations (transformedExpression))
          .Return (new ExpressionTransformation[0]);
      providerMock.Replay ();

      var result = TransformingExpressionTreeVisitor.Transform (inputExpression, providerMock);

      providerMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (transformedExpression));
    }

    [Test]
    public void Transform_ChildrenTransformedBeforeParent ()
    {
      var inputChildExpression = CreateSimpleExpression(0);
      var transformedChildExpression = CreateSimpleExpression (1);

      var inputParentExpression = Expression.UnaryPlus (inputChildExpression);
      var transformedParentExpression = CreateSimpleExpression(2);

      var providerMock = MockRepository.GenerateStrictMock<IExpressionTranformationProvider> ();
      providerMock
          .Expect (mock => mock.GetTransformations (inputChildExpression))
          .Return (new ExpressionTransformation[] { expr => transformedChildExpression });
      providerMock
          .Expect (mock => mock.GetTransformations (transformedChildExpression))
          .Return (new ExpressionTransformation[0]);
      providerMock
          .Expect (mock => mock.GetTransformations (
              Arg<Expression>.Matches (expr => expr is UnaryExpression && ((UnaryExpression) expr).Operand == transformedChildExpression)))
          .Return (new ExpressionTransformation[] { expr => transformedParentExpression });
      providerMock
          .Expect (mock => mock.GetTransformations (transformedParentExpression))
          .Return (new ExpressionTransformation[0]);
      providerMock.Replay ();

      var result = TransformingExpressionTreeVisitor.Transform (inputParentExpression, providerMock);

      providerMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (transformedParentExpression));
    }

    [Test]
    public void Transform_NullExpression ()
    {
      var providerMock = MockRepository.GenerateStrictMock<IExpressionTranformationProvider> ();
      providerMock.Replay ();

      var result = TransformingExpressionTreeVisitor.Transform (null, providerMock);

      providerMock.VerifyAllExpectations ();
      Assert.That (result, Is.Null);
    }

    private Expression CreateSimpleExpression (int id)
    {
      return Expression.Constant (id);
    }
  }
}