// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests
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

    protected Expression InvokeAndCheckVisitExpression (string methodName, Expression expression)
    {
      return (Expression) InvokeAndCheckVisitObject (methodName, expression);
    }

    protected object InvokeAndCheckVisitObject (string methodName, object argument)
    {
      return InvokeAndCheckVisitMethod (delegate { return InvokeVisitMethod (methodName, argument); }, argument);
    }

    protected ReadOnlyCollection<T> InvokeAndCheckVisitExpressionList<T> (ReadOnlyCollection<T> expressions, string methodName) where T : Expression
    {
      return InvokeAndCheckVisitMethod (arg => VisitorMock.VisitAndConvert (expressions, methodName), expressions);
    }

    protected T InvokeAndCheckVisitAndConvertExpression<T> (T expression, string methodName) where T : Expression
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
