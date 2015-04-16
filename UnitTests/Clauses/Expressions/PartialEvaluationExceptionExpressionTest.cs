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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses.Expressions
{
  [TestFixture]
  public class PartialEvaluationExceptionExpressionTest
  {
    private InvalidOperationException _exception;
    private Expression _evaluatedExpression;

    private PartialEvaluationExceptionExpression _exceptionExpression;

    [SetUp]
    public void SetUp ()
    {
      _exception = new InvalidOperationException ("What");
      _evaluatedExpression = ExpressionHelper.CreateExpression (typeof (double));

      _exceptionExpression = new PartialEvaluationExceptionExpression (_exception, _evaluatedExpression);
    }

    [Test]
    public void NodeType ()
    {
      Assert.That (PartialEvaluationExceptionExpression.ExpressionType, Is.EqualTo ((ExpressionType) 100004));
      ExtensionExpressionTestHelper.CheckUniqueNodeType (
          typeof (PartialEvaluationExceptionExpression), PartialEvaluationExceptionExpression.ExpressionType);
    }
    
    [Test]
    public void Initialization ()
    {
      Assert.That (_exceptionExpression.Type, Is.SameAs (typeof (double)));
      Assert.That (_exceptionExpression.NodeType, Is.EqualTo (PartialEvaluationExceptionExpression.ExpressionType));
      Assert.That (_exceptionExpression.Exception, Is.SameAs (_exception));
      Assert.That (_exceptionExpression.EvaluatedExpression, Is.SameAs (_evaluatedExpression));
    }

    [Test]
    public void CanReduce ()
    {
      Assert.That (_exceptionExpression.CanReduce, Is.True);
    }

    [Test]
    public void Reduce ()
    {
      var result = _exceptionExpression.Reduce ();

      Assert.That (result, Is.SameAs (_evaluatedExpression));
    }

    [Test]
    public void VisitChildren_ReturnsSameExpression ()
    {
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.Visit (_evaluatedExpression))
          .Return (_evaluatedExpression);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_exceptionExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (_exceptionExpression));
    }

    [Test]
    public void VisitChildren_ReturnsNewExpression ()
    {
      var newExpression = Expression.Equal (Expression.Constant ("string1"), Expression.Constant ("string"));
      var visitorMock = MockRepository.GenerateStrictMock<ExpressionTreeVisitor> ();

      visitorMock
          .Expect (mock => mock.Visit (_evaluatedExpression))
          .Return (newExpression);
      visitorMock.Replay ();

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_exceptionExpression, visitorMock);

      visitorMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (_exceptionExpression));
      Assert.That (((PartialEvaluationExceptionExpression) result).Exception, Is.SameAs (_exception));
      Assert.That (((PartialEvaluationExceptionExpression) result).EvaluatedExpression, Is.SameAs (newExpression));
    }

    [Test]
    public void Accept_VisitorSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorSupportingType<PartialEvaluationExceptionExpression, IPartialEvaluationExceptionExpressionVisitor> (
          _exceptionExpression,
          mock => mock.VisitPartialEvaluationExceptionExpression (_exceptionExpression));
    }

    [Test]
    public void Accept_VisitorNotSupportingExpressionType ()
    {
      ExtensionExpressionTestHelper.CheckAcceptForVisitorNotSupportingType (_exceptionExpression);
    }

    [Test]
    public void To_String ()
    {
      var result = _exceptionExpression.ToString ();

      Assert.That (result, Is.EqualTo ("PartialEvalException (InvalidOperationException (\"What\"), " + _evaluatedExpression + ")"));
    }
  }
}