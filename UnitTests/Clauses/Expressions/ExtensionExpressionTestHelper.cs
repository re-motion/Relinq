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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses.Expressions
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
          StringUtility.Join (", ", matches.Select (field => string.Format ("{0} ({1})", field.Field.DeclaringType.Name, field.Field.Name))));
    }
  }
}