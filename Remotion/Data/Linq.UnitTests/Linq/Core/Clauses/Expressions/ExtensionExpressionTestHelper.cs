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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestUtilities;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.Expressions
{
  public static class ExtensionExpressionTestHelper
  {
    public static void CheckAcceptForVisitorSupportingType<TExpression, TVisitorInterface> (
        TExpression expression,
        Func<TVisitorInterface, Expression> visitMethodCall) where TExpression : ExtensionExpression
    {
      var mockRepository = new MockRepository ();
      var visitorMock = mockRepository.StrictMultiMock<ExpressionTreeVisitor> (typeof (TVisitorInterface));

      var returnedExpression = Expression.Constant (0);

      visitorMock
          .Expect (mock => visitMethodCall ((TVisitorInterface) (object) mock))
          .Return (returnedExpression);
      visitorMock.Replay ();

      var result = expression.Accept (visitorMock);

      visitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (returnedExpression));
    }

    public static void CheckAcceptForVisitorNotSupportingType<TExpression> (TExpression expression) where TExpression : ExtensionExpression
    {
      var mockRepository = new MockRepository ();
      var visitorMock = mockRepository.StrictMock<ExpressionTreeVisitor> ();

      var returnedExpression = Expression.Constant (0);

      visitorMock
          .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitExtensionExpression", expression))
          .Return (returnedExpression);
      visitorMock.Replay ();

      var result = expression.Accept (visitorMock);

      visitorMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (returnedExpression));
    }

    public static Expression CallVisitChildren (ExtensionExpression target, ExpressionTreeVisitor visitor)
    {
      return (Expression) PrivateInvoke.InvokeNonPublicMethod (target, "VisitChildren", visitor);
    }
  }
}