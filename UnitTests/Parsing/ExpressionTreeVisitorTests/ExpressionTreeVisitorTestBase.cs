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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Parsing;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitorTests
{
  public class ExpressionTreeVisitorTestBase
  {
    private MockRepository _mockRepository;
    private ExpressionTreeVisitor _visitorMock;

    [SetUp]
    public virtual void Setup ()
    {
      _mockRepository = new MockRepository ();
      _visitorMock = _mockRepository.StrictMock<ExpressionTreeVisitor> ();
    }

    protected MockRepository MockRepository
    {
      get { return _mockRepository; }
      set { _mockRepository = value; }
    }

    public ExpressionTreeVisitor VisitorMock
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

    protected ReadOnlyCollection<T> InvokeAndCheckVisitAndConvertList<T> (ReadOnlyCollection<T> expressions, string methodName) where T : Expression
    {
      return InvokeAndCheckVisitMethod (arg => VisitorMock.VisitAndConvert (expressions, methodName), expressions);
    }

    protected T InvokeAndCheckVisitAndConvert<T> (T expression, string methodName) where T : Expression
    {
      return InvokeAndCheckVisitMethod (arg => VisitorMock.VisitAndConvert (expression, methodName), expression);
    }

    protected ReadOnlyCollection<MemberBinding> InvokeAndCheckVisitMemberBindingList (ReadOnlyCollection<MemberBinding> expressions)
    {
      return InvokeAndCheckVisitMethod (
          arg => (ReadOnlyCollection<MemberBinding>) InvokeVisitMethod ("VisitMemberBindingList", expressions), expressions);
    }

    protected ReadOnlyCollection<ElementInit> InvokeAndCheckVisitElementInitList (ReadOnlyCollection<ElementInit> expressions)
    {
      return InvokeAndCheckVisitMethod (
          delegate { return (ReadOnlyCollection<ElementInit>) InvokeVisitMethod ("VisitElementInitList", expressions); }, expressions);
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
      return _visitorMock.GetType ().GetMethod (methodName, BindingFlags.NonPublic | BindingFlags.Instance).Invoke (_visitorMock, new[] { argument });
    }

    protected T InvokeVisitAndConvertMethod<T> (T expression, string methodName) where T : Expression
    {
      return (T) _visitorMock.GetType ().GetMethod ("VisitAndConvert", BindingFlags.NonPublic | BindingFlags.Instance)
                     .MakeGenericMethod (typeof (T))
                     .Invoke (_visitorMock, new object[] { expression, methodName });
    }
  }
}
