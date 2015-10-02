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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitorTests
{
  [TestFixture]
  public class RelinqExpressionVisitor_NewExpressionsTest : RelinqExpressionVisitorTestBase
  {
    [Test]
    public void VisitNew_Unchanged ()
    {
      var expression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);
      var argument = expression.Arguments.Single();
      Expect.Call (VisitorMock.Visit (argument)).Return (argument);
      var result = (NewExpression) InvokeAndCheckVisit ("VisitNew", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNew_ChangedArguments ()
    {
      var expression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);
      var argument = expression.Arguments.Single();
      Expression newArgument = Expression.Constant (214578);
      Expect.Call (VisitorMock.Visit (argument)).Return (newArgument);
      var result = (NewExpression) InvokeAndCheckVisit ("VisitNew", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.New));
      Assert.That (result.Arguments, Is.EqualTo (new[] { newArgument }));
    }

    [Test]
    public void VisitNew_ChangedArguments_NoMembers ()
    {
      NewExpression expression = Expression.New (TypeForNewExpression.GetConstructor (typeof(int)), Expression.Constant (0));
      var argument = expression.Arguments.Single();

      var newArgument = Expression.Constant (214578);
      Expect.Call (VisitorMock.Visit (argument)).Return (newArgument);
      var result = (NewExpression) InvokeAndCheckVisit ("VisitNew", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.New));
      Assert.That (result.Arguments, Is.EqualTo (new[] { newArgument }));
    }

    [Test]
    public void VisitNew_ChangedArguments_WithMembers ()
    {
      NewExpression expression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof(int)),
          new Expression[] { Expression.Constant (0) },
          typeof (TypeForNewExpression).GetProperty ("A"));
      var argument = expression.Arguments.Single();

      var newArgument = Expression.Constant (214578);
      Expect.Call (VisitorMock.Visit (argument)).Return (newArgument);
      var result = (NewExpression) InvokeAndCheckVisit ("VisitNew", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.New));
      Assert.That (result.Arguments, Is.EqualTo (new[] { newArgument }));
      Assert.That (result.Members, Is.SameAs (expression.Members));
    }

    [Test]
    public void VisitNew_ChangedArguments_WithMembers_AndConversionRequired ()
    {
      NewExpression expression = Expression.New (
          typeof (KeyValuePair<object, object>).GetConstructor (new[] { typeof (object), typeof (object) }),
          new Expression[] { Expression.Constant (null), Expression.Constant (null) },
          typeof (KeyValuePair<object, object>).GetProperty ("Key"), typeof (KeyValuePair<object, object>).GetProperty ("Value"));
      var argument1 = expression.Arguments[0];
      var argument2 = expression.Arguments[1];

      var newArgument1 = Expression.Constant ("testKey");
      var newArgument2 = Expression.Constant ("testValue");
      Expect.Call (VisitorMock.Visit (argument1)).Return (newArgument1);
      Expect.Call (VisitorMock.Visit (argument2)).Return (newArgument2);
      
      var result = (NewExpression) InvokeAndCheckVisit ("VisitNew", expression);

      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.New));
      
      Assert.That (result.Arguments.Count, Is.EqualTo (2));
      var expectedArgument1 = Expression.Convert (newArgument1, typeof (object));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedArgument1, result.Arguments[0]);
      var expectedArgument2 = Expression.Convert (newArgument2, typeof (object));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedArgument2, result.Arguments[1]);
      
      Assert.That (result.Members, Is.SameAs (expression.Members));
    }
  }
}