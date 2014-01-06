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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.TestUtilities;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions
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

    public static void CheckUniqueNodeType (Type expressionType, ExpressionType nodeType)
    {
      Assert.That (Enum.IsDefined (typeof (ExpressionType), nodeType), Is.False, "Type is one of the standard types");
      var allExpressionTypeFields = from asm in AppDomain.CurrentDomain.GetAssemblies()
                                    from type in asm.GetTypes()
                                    from field in type.GetFields (BindingFlags.Public | BindingFlags.Static)
                                    where field.FieldType == typeof (ExpressionType)
                                    select field;
      var fieldValues = from field in allExpressionTypeFields
                        select new { Field = field, Value = field.GetValue (null) };
      var lookup = fieldValues.ToLookup (x => x.Value);
      var matches = lookup[nodeType].Where (field => field.Field.DeclaringType != expressionType).ToArray();
      Assert.That (
          matches, 
          Is.Empty, 
          "'{0}' declares the same node type as {1}.", 
          expressionType.Name, 
          string.Join (", ", matches.Select (field => string.Format ("{0} ({1})", field.Field.DeclaringType.Name, field.Field.Name))));
    }
  }
}