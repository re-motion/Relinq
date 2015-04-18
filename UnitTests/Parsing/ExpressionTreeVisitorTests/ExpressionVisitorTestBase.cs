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
using Remotion.Linq.Parsing;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitorTests
{
  public class ExpressionVisitorTestBase
  {
    private MockRepository _mockRepository;
    private RelinqExpressionVisitor _visitorMock;

    [SetUp]
    public virtual void Setup ()
    {
      _mockRepository = new MockRepository ();
      _visitorMock = _mockRepository.StrictMock<RelinqExpressionVisitor> ();
    }

    protected MockRepository MockRepository
    {
      get { return _mockRepository; }
      set { _mockRepository = value; }
    }

    public RelinqExpressionVisitor VisitorMock
    {
      get { return _visitorMock; }
    }

    protected Expression InvokeAndCheckVisit (string methodName, Expression expression)
    {
      return (Expression) InvokeAndCheckVisitObject (methodName, expression);
    }

    protected object InvokeAndCheckVisitObject (string methodName, object argument)
    {
      return InvokeAndCheckVisitMethod (delegate { return InvokeVisitMethod (methodName, argument); }, argument);
    }

    protected T InvokeVisitAndConvert<T> (T expression, string methodName) where T : Expression
    {
      _mockRepository.ReplayAll ();

      T result = VisitorMock.VisitAndConvert (expression, methodName);
      _mockRepository.VerifyAll ();

      return result;
    }

    private R InvokeAndCheckVisitMethod<A, R> (Func<A, R> visitMethod, A argument)
    {
      Expect.Call (visitMethod (argument)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);

      _mockRepository.ReplayAll ();

      R result = visitMethod (argument);
      _mockRepository.VerifyAll ();

      return result;
    }

    protected object InvokeVisitMethod (string methodName, object argument)
    {
      var methodInfo = _visitorMock.GetType ().GetMethod (methodName, BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (methodInfo, Is.Not.Null);

      if (methodInfo.ContainsGenericParameters)
        methodInfo = methodInfo.MakeGenericMethod (argument.GetType().GetGenericArguments());
      return methodInfo.Invoke (_visitorMock, new[] { argument });
    }
  }
}
