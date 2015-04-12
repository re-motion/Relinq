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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitorTests
{
  [TestFixture]
  public class ExpressionTreeVisitor_SpecificExpressionsTest : ExpressionTreeVisitorTestBase
  {
    [Test]
    public void VisitUnary_Unchanges ()
    {
      var expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.UnaryPlus);
      Expression expectedNextVisit = expression.Operand;
      Expect.Call (VisitorMock.VisitExpression (expectedNextVisit)).Return (expectedNextVisit);

      Assert.That (InvokeAndCheckVisitExpression ("VisitUnary", expression), Is.SameAs (expression));
    }

    [Test]
    public void VisitUnary_UnaryPlus_Changes ()
    {
      var expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.UnaryPlus);
      Expression expectedNextVisit = expression.Operand;
      Expression newOperand = Expression.Constant (1);
      Expect.Call (VisitorMock.VisitExpression (expectedNextVisit)).Return (newOperand);

      var result = (UnaryExpression) InvokeAndCheckVisitExpression ("VisitUnary", expression);
      Assert.That (result.Operand, Is.SameAs (newOperand));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.UnaryPlus));
    }

    [Test]
    public void VisitUnary_Negate_Changes ()
    {
      var expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Negate);
      Expression expectedNextVisit = expression.Operand;
      Expression newOperand = Expression.Constant (1);
      Expect.Call (VisitorMock.VisitExpression (expectedNextVisit)).Return (newOperand);

      var result = (UnaryExpression) InvokeAndCheckVisitExpression ("VisitUnary", expression);
      Assert.That (result.Operand, Is.SameAs (newOperand));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Negate));
    }

    [Test]
    public void VisitTypeBinary_Unchanged ()
    {
      var expression = (TypeBinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.TypeIs);
      Expect.Call (VisitorMock.VisitExpression (expression.Expression)).Return (expression.Expression);
      var result = (TypeBinaryExpression) InvokeAndCheckVisitExpression ("VisitTypeBinary", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitTypeBinary_Changed ()
    {
      var expression = (TypeBinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.TypeIs);
      Expression newExpression = Expression.Constant (1);
      Expect.Call (VisitorMock.VisitExpression (expression.Expression)).Return (newExpression);
      var result = (TypeBinaryExpression) InvokeAndCheckVisitExpression ("VisitTypeBinary", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.TypeIs));
      Assert.That (result.Expression, Is.SameAs (newExpression));
    }

    [Test]
    public void VisitConstant ()
    {
      var expression = (ConstantExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Constant);
      var result = (ConstantExpression) InvokeAndCheckVisitExpression ("VisitConstant", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitConditional_Unchanged ()
    {
      var expression = (ConditionalExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Conditional);
      Expect.Call (VisitorMock.VisitExpression (expression.Test)).Return (expression.Test);
      Expect.Call (VisitorMock.VisitExpression (expression.IfTrue)).Return (expression.IfTrue);
      Expect.Call (VisitorMock.VisitExpression (expression.IfFalse)).Return (expression.IfFalse);
      var result = (ConditionalExpression) InvokeAndCheckVisitExpression ("VisitConditional", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitConditional_ChangedTest ()
    {
      var expression = (ConditionalExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Conditional);
      Expression newTest = Expression.Constant (true);
      Expect.Call (VisitorMock.VisitExpression (expression.Test)).Return (newTest);
      Expect.Call (VisitorMock.VisitExpression (expression.IfTrue)).Return (expression.IfTrue);
      Expect.Call (VisitorMock.VisitExpression (expression.IfFalse)).Return (expression.IfFalse);
      var result = (ConditionalExpression) InvokeAndCheckVisitExpression ("VisitConditional", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Conditional));
      Assert.That (result.Test, Is.SameAs (newTest));
      Assert.That (result.IfTrue, Is.SameAs (expression.IfTrue));
      Assert.That (result.IfFalse, Is.SameAs (expression.IfFalse));
    }

    [Test]
    public void VisitConditional_ChangedFalse ()
    {
      var expression = (ConditionalExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Conditional);
      Expression newFalse = Expression.Constant (1);
      Expect.Call (VisitorMock.VisitExpression (expression.Test)).Return (expression.Test);
      Expect.Call (VisitorMock.VisitExpression (expression.IfTrue)).Return (expression.IfTrue);
      Expect.Call (VisitorMock.VisitExpression (expression.IfFalse)).Return (newFalse);
      var result = (ConditionalExpression) InvokeAndCheckVisitExpression ("VisitConditional", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Conditional));
      Assert.That (result.Test, Is.SameAs (expression.Test));
      Assert.That (result.IfTrue, Is.SameAs (expression.IfTrue));
      Assert.That (result.IfFalse, Is.SameAs (newFalse));
    }

    [Test]
    public void VisitConditional_ChangedTrue ()
    {
      var expression = (ConditionalExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Conditional);
      Expression newTrue = Expression.Constant (1);
      Expect.Call (VisitorMock.VisitExpression (expression.Test)).Return (expression.Test);
      Expect.Call (VisitorMock.VisitExpression (expression.IfTrue)).Return (newTrue);
      Expect.Call (VisitorMock.VisitExpression (expression.IfFalse)).Return (expression.IfFalse);
      var result = (ConditionalExpression) InvokeAndCheckVisitExpression ("VisitConditional", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Conditional));
      Assert.That (result.Test, Is.SameAs (expression.Test));
      Assert.That (result.IfTrue, Is.SameAs (newTrue));
      Assert.That (result.IfFalse, Is.SameAs (expression.IfFalse));
    }

#if !NET_3_5
    [Test]
    public void VisitConditional_WithType_AndChanges ()
    {
      var test = Expression.Constant (true);
      var ifTrue = Expression.Constant (null, typeof (object));
      var ifFalse = Expression.Constant ("false", typeof (string));
      var expression = Expression.Condition (test, ifTrue, ifFalse, typeof (object));
      
      Expression newIfFalse = Expression.Constant ("FALSE", typeof (string));
      VisitorMock.Expect (mock => mock.VisitExpression (expression.Test)).Return (expression.Test);
      VisitorMock.Expect (mock => mock.VisitExpression (expression.IfTrue)).Return (expression.IfTrue);
      VisitorMock.Expect (mock => mock.VisitExpression (expression.IfFalse)).Return (newIfFalse);

      var result = (ConditionalExpression) InvokeAndCheckVisitExpression ("VisitConditional", expression);

      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Conditional));
      Assert.That (result.Type, Is.SameAs (typeof (object)));
      Assert.That (result.Test, Is.SameAs (expression.Test));
      Assert.That (result.IfTrue, Is.SameAs (ifTrue));
      Assert.That (result.IfFalse, Is.SameAs (newIfFalse));
    }
#endif

    [Test]
    public void VisitParameter ()
    {
      var expression = (ParameterExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Parameter);
      var result = (ParameterExpression) InvokeAndCheckVisitExpression ("VisitParameter", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitLambda_Unchanged ()
    {
      var expression = (LambdaExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Lambda);
      Expect.Call (VisitorMock.VisitExpression (expression.Body)).Return (expression.Body);
      Expect.Call (VisitorMock.VisitAndConvert (expression.Parameters, "VisitLambda")).Return (expression.Parameters);
      var result = (LambdaExpression) InvokeAndCheckVisitExpression ("VisitLambda", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitLambda_ChangedBody ()
    {
      var expression = (LambdaExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Lambda);
      Expression newBody = Expression.Constant (1);
      Expect.Call (VisitorMock.VisitExpression (expression.Body)).Return (newBody);
      Expect.Call (VisitorMock.VisitAndConvert (expression.Parameters, "VisitLambda")).Return (expression.Parameters);
      var result = (LambdaExpression) InvokeAndCheckVisitExpression ("VisitLambda", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Lambda));
      Assert.That (result.Body, Is.SameAs (newBody));
      Assert.That (result.Parameters, Is.SameAs (expression.Parameters));
    }

    [Test]
    public void VisitLambda_ChangedParameters ()
    {
      var expression = (LambdaExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Lambda);
      ReadOnlyCollection<ParameterExpression> newParameters = new List<ParameterExpression> { Expression.Parameter (typeof (int), "i") }.AsReadOnly ();
      Expect.Call (VisitorMock.VisitExpression (expression.Body)).Return (expression.Body);
      Expect.Call (VisitorMock.VisitAndConvert (expression.Parameters, "VisitLambda")).Return (newParameters);
      var result = (LambdaExpression) InvokeAndCheckVisitExpression ("VisitLambda", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Lambda));
      Assert.That (result.Parameters, Is.EqualTo (newParameters));
      Assert.That (result.Body, Is.SameAs (expression.Body));
    }

    [Test]
    public void VisitMethodCall_Unchanged ()
    {
      var expression = (MethodCallExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Call);
      Expect.Call (VisitorMock.VisitExpression (expression.Object)).Return (expression.Object);
      Expect.Call (VisitorMock.VisitAndConvert (expression.Arguments, "VisitMethodCall")).Return (expression.Arguments);
      var result = (MethodCallExpression) InvokeAndCheckVisitExpression ("VisitMethodCall", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitMethodCall_ChangedObject ()
    {
      var expression = (MethodCallExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Call);
      Expression newObject = Expression.Constant (1);
      Expect.Call (VisitorMock.VisitExpression (expression.Object)).Return (newObject);
      Expect.Call (VisitorMock.VisitAndConvert (expression.Arguments, "VisitMethodCall")).Return (expression.Arguments);
      var result = (MethodCallExpression) InvokeAndCheckVisitExpression ("VisitMethodCall", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Call));
      Assert.That (result.Object, Is.SameAs (newObject));
      Assert.That (result.Arguments, Is.SameAs (expression.Arguments));
    }

    [Test]
    public void VisitMethodCall_ChangedArguments ()
    {
      var expression = (MethodCallExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Call);
      ReadOnlyCollection<Expression> newParameters = new List<Expression> { Expression.Constant (214578) }.AsReadOnly ();
      Expect.Call (VisitorMock.VisitExpression (expression.Object)).Return (expression.Object);
      Expect.Call (VisitorMock.VisitAndConvert (expression.Arguments, "VisitMethodCall")).Return (newParameters);
      var result = (MethodCallExpression) InvokeAndCheckVisitExpression ("VisitMethodCall", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Call));
      Assert.That (result.Arguments, Is.EqualTo (newParameters));
      Assert.That (result.Object, Is.SameAs (expression.Object));
    }

    [Test]
    public void VisitInvocationExpression_Unchanged ()
    {
      var expression = (InvocationExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Invoke);
      Expect.Call (VisitorMock.VisitExpression (expression.Expression)).Return (expression.Expression);
      Expect.Call (VisitorMock.VisitAndConvert (expression.Arguments, "VisitInvocationExpression")).Return (expression.Arguments);
      var result = (InvocationExpression) InvokeAndCheckVisitExpression ("VisitInvocationExpression", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitInvocationExpression_ChangedObject ()
    {
      var expression = (InvocationExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Invoke);
      Expression newExpression = Expression.Lambda (Expression.Constant (1), Expression.Parameter (typeof (int), "i"));
      Expect.Call (VisitorMock.VisitExpression (expression.Expression)).Return (newExpression);
      Expect.Call (VisitorMock.VisitAndConvert (expression.Arguments, "VisitInvocationExpression")).Return (expression.Arguments);
      var result = (InvocationExpression) InvokeAndCheckVisitExpression ("VisitInvocationExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Invoke));
      Assert.That (result.Expression, Is.SameAs (newExpression));
      Assert.That (result.Arguments, Is.SameAs (expression.Arguments));
    }

    [Test]
    public void VisitInvocationExpression_ChangedArguments ()
    {
      var expression = (InvocationExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Invoke);
      ReadOnlyCollection<Expression> newParameters = new List<Expression> { Expression.Constant (214578) }.AsReadOnly ();
      Expect.Call (VisitorMock.VisitExpression (expression.Expression)).Return (expression.Expression);
      Expect.Call (VisitorMock.VisitAndConvert (expression.Arguments, "VisitInvocationExpression")).Return (newParameters);
      var result = (InvocationExpression) InvokeAndCheckVisitExpression ("VisitInvocationExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Invoke));
      Assert.That (result.Arguments, Is.EqualTo (newParameters));
      Assert.That (result.Expression, Is.SameAs (expression.Expression));
    }

    [Test]
    public void VisitMemberExpression_Unchanged ()
    {
      var expression = (MemberExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberAccess);
      Expect.Call (VisitorMock.VisitExpression (expression.Expression)).Return (expression.Expression);
      var result = (MemberExpression) InvokeAndCheckVisitExpression ("VisitMemberExpression", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitMemberExpression_ChangedExpression ()
    {
      var expression = (MemberExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberAccess);
      Expression newExpression = Expression.Constant (DateTime.Now);
      Expect.Call (VisitorMock.VisitExpression (expression.Expression)).Return (newExpression);
      var result = (MemberExpression) InvokeAndCheckVisitExpression ("VisitMemberExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.MemberAccess));
      Assert.That (result.Expression, Is.SameAs (newExpression));
    }

    [Test]
    public void VisitNewExpression_Unchanged ()
    {
      var expression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);
      Expect.Call (VisitorMock.VisitAndConvert (expression.Arguments, "VisitNewExpression")).Return (expression.Arguments);
      var result = (NewExpression) InvokeAndCheckVisitExpression ("VisitNewExpression", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNewExpression_ChangedArguments ()
    {
      var expression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);
      ReadOnlyCollection<Expression> newArguments = new List<Expression> { Expression.Constant (214578) }.AsReadOnly ();
      Expect.Call (VisitorMock.VisitAndConvert (expression.Arguments, "VisitNewExpression")).Return (newArguments);
      var result = (NewExpression) InvokeAndCheckVisitExpression ("VisitNewExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.New));
      Assert.That (result.Arguments, Is.EqualTo (newArguments));
    }

    [Test]
    public void VisitNewExpression_ChangedArguments_NoMembers ()
    {
      NewExpression expression = Expression.New (TypeForNewExpression.GetConstructor (typeof(int)), Expression.Constant (0));

      var newArguments = new List<Expression> { Expression.Constant (214578) }.AsReadOnly ();
      Expect.Call (VisitorMock.VisitAndConvert (expression.Arguments, "VisitNewExpression")).Return (newArguments);
      var result = (NewExpression) InvokeAndCheckVisitExpression ("VisitNewExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.New));
      Assert.That (result.Arguments, Is.EqualTo (newArguments));
    }

    [Test]
    public void VisitNewExpression_ChangedArguments_WithMembers ()
    {
      NewExpression expression = Expression.New (
          TypeForNewExpression.GetConstructor (typeof(int)),
          new Expression[] { Expression.Constant (0) },
          typeof (TypeForNewExpression).GetProperty ("A"));

      var newArguments = new List<Expression> { Expression.Constant (214578) }.AsReadOnly ();
      Expect.Call (VisitorMock.VisitAndConvert (expression.Arguments, "VisitNewExpression")).Return (newArguments);
      var result = (NewExpression) InvokeAndCheckVisitExpression ("VisitNewExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.New));
      Assert.That (result.Arguments, Is.EqualTo (newArguments));
      Assert.That (result.Members, Is.SameAs (expression.Members));
    }

    [Test]
    public void VisitNewExpression_ChangedArguments_WithMembers_AndConversionRequired ()
    {
      NewExpression expression = Expression.New (
          typeof (KeyValuePair<object, object>).GetConstructor (new[] { typeof (object), typeof (object) }),
          new Expression[] { Expression.Constant (null), Expression.Constant (null) },
          typeof (KeyValuePair<object, object>).GetProperty ("Key"), typeof (KeyValuePair<object, object>).GetProperty ("Value"));

      var newArguments = new List<Expression> { Expression.Constant ("testKey"), Expression.Constant ("testValue") }.AsReadOnly ();
      Expect.Call (VisitorMock.VisitAndConvert (expression.Arguments, "VisitNewExpression")).Return (newArguments);
      
      var result = (NewExpression) InvokeAndCheckVisitExpression ("VisitNewExpression", expression);

      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.New));
      
      Assert.That (result.Arguments.Count, Is.EqualTo (2));
      var expectedArgument1 = Expression.Convert (newArguments[0], typeof (object));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedArgument1, result.Arguments[0]);
      var expectedArgument2 = Expression.Convert (newArguments[1], typeof (object));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedArgument2, result.Arguments[1]);
      
      Assert.That (result.Members, Is.SameAs (expression.Members));
    }

    [Test]
    public void VisitNewArrayExpression_Unchanged ()
    {
      var expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayInit);
      Expect.Call (VisitorMock.VisitAndConvert (expression.Expressions, "VisitNewArrayExpression")).Return (expression.Expressions);
      var result = (NewArrayExpression) InvokeAndCheckVisitExpression ("VisitNewArrayExpression", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNewArrayExpression_Init_Changed ()
    {
      var expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayInit);
      ReadOnlyCollection<Expression> newExpressions = new List<Expression> { Expression.Constant (214578) }.AsReadOnly ();
      Expect.Call (VisitorMock.VisitAndConvert (expression.Expressions, "VisitNewArrayExpression")).Return (newExpressions);
      var result = (NewArrayExpression) InvokeAndCheckVisitExpression ("VisitNewArrayExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.NewArrayInit));
      Assert.That (result.Expressions, Is.EqualTo (newExpressions));
      Assert.That (result.Type, Is.EqualTo (typeof (int[])));
    }

    [Test]
    public void VisitNewArrayExpression_Bounds_Changed ()
    {
      var expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayBounds);
      ReadOnlyCollection<Expression> newExpressions = new List<Expression> (new Expression[] { Expression.Constant (214578) }).AsReadOnly ();
      Expect.Call (VisitorMock.VisitAndConvert (expression.Expressions, "VisitNewArrayExpression")).Return (newExpressions);
      var result = (NewArrayExpression) InvokeAndCheckVisitExpression ("VisitNewArrayExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.NewArrayBounds));
      Assert.That (result.Expressions, Is.EqualTo (newExpressions));
#if !NET_3_5
      Assert.That (result.Type, Is.EqualTo (typeof (int).MakeArrayType()));
#else
      Assert.That (result.Type, Is.EqualTo (typeof (int).MakeArrayType (1)));
#endif
    }

    [Test]
    public void VisitMemberInitExpression_Unchanged ()
    {
      var expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      Expect.Call (VisitorMock.VisitExpression (expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitMemberBindingList", expression.Bindings)).Return (expression.Bindings);
      var result = (MemberInitExpression) InvokeAndCheckVisitExpression ("VisitMemberInitExpression", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "MemberInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.")]
    public void VisitMemberInitExpression_InvalidNewExpression ()
    {
      var expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      Expect.Call (VisitorMock.VisitExpression (expression.NewExpression)).Return (Expression.Constant (0));
      Expect.Call (InvokeVisitMethod ("VisitMemberBindingList", expression.Bindings)).Return (expression.Bindings);
      try
      {
        InvokeAndCheckVisitExpression ("VisitMemberInitExpression", expression);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException;
      }
    }

    [Test]
    public void VisitMemberInitExpression_ChangedNewExpression ()
    {
      var expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      NewExpression newNewExpression = Expression.New (typeof (List<int>));
      Expect.Call (VisitorMock.VisitExpression (expression.NewExpression)).Return (newNewExpression);
      Expect.Call (InvokeVisitMethod ("VisitMemberBindingList", expression.Bindings)).Return (expression.Bindings);
      var result = (MemberInitExpression) InvokeAndCheckVisitExpression ("VisitMemberInitExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.MemberInit));
      Assert.That (result.NewExpression, Is.SameAs (newNewExpression));
      Assert.That (result.Bindings, Is.SameAs (expression.Bindings));
    }

    [Test]
    public void VisitMemberInitExpression_ChangedBindings ()
    {
      var expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      var capacityProperty = expression.NewExpression.Constructor.DeclaringType.GetProperty ("Capacity");

      ReadOnlyCollection<MemberBinding> newBindings = new List<MemberBinding> { Expression.Bind (capacityProperty, Expression.Constant (214578)) }.AsReadOnly ();
      Expect.Call (VisitorMock.VisitExpression (expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitMemberBindingList", expression.Bindings)).Return (newBindings);
      var result = (MemberInitExpression) InvokeAndCheckVisitExpression ("VisitMemberInitExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.MemberInit));
      Assert.That (result.Bindings, Is.EqualTo (newBindings));
      Assert.That (result.NewExpression, Is.SameAs (expression.NewExpression));
    }

    [Test]
    public void VisitListInitExpression_Unchanged ()
    {
      var expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      Expect.Call (VisitorMock.VisitExpression (expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitElementInitList", expression.Initializers)).Return (expression.Initializers);
      var result = (ListInitExpression) InvokeAndCheckVisitExpression ("VisitListInitExpression", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "ListInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.")]
    public void VisitListInitExpression_InvalidNewExpression ()
    {
      var expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      Expect.Call (VisitorMock.VisitExpression (expression.NewExpression)).Return (Expression.Constant (0));
      Expect.Call (InvokeVisitMethod ("VisitElementInitList", expression.Initializers)).Return (expression.Initializers);
      try
      {
        InvokeAndCheckVisitExpression ("VisitListInitExpression", expression);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException;
      }
    }

    [Test]
    public void VisitListInitExpression_ChangedNewExpression ()
    {
      var expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      NewExpression newNewExpression = Expression.New (typeof (List<int>));
      Expect.Call (VisitorMock.VisitExpression (expression.NewExpression)).Return (newNewExpression);
      Expect.Call (InvokeVisitMethod ("VisitElementInitList", expression.Initializers)).Return (expression.Initializers);
      var result = (ListInitExpression) InvokeAndCheckVisitExpression ("VisitListInitExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.ListInit));
      Assert.That (result.NewExpression, Is.SameAs (newNewExpression));
      Assert.That (result.Initializers, Is.SameAs (expression.Initializers));
    }

    [Test]
    public void VisitListInitExpression_ChangedInitializers ()
    {
      var expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      ReadOnlyCollection<ElementInit> newInitializers =
          new List<ElementInit> (new[] { Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (214578)) }).AsReadOnly ();
      Expect.Call (VisitorMock.VisitExpression (expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitElementInitList", expression.Initializers)).Return (newInitializers);
      var result = (ListInitExpression) InvokeAndCheckVisitExpression ("VisitListInitExpression", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.ListInit));
      Assert.That (result.Initializers, Is.EqualTo (newInitializers));
      Assert.That (result.NewExpression, Is.SameAs (expression.NewExpression));
    }

    [Test]
    public void VisitElementInit_Unchanged ()
    {
      ElementInit elementInit = ExpressionInstanceCreator.CreateElementInit();
      Expect.Call (VisitorMock.VisitAndConvert (elementInit.Arguments, "VisitElementInit")).Return (elementInit.Arguments);

      var result = (ElementInit) InvokeAndCheckVisitObject ("VisitElementInit", elementInit);
      Assert.That (result, Is.SameAs (elementInit));
    }

    [Test]
    public void VisitElementInit_Changed ()
    {
      ElementInit elementInit = ExpressionInstanceCreator.CreateElementInit ();
      ReadOnlyCollection<Expression> newArguments = new List<Expression> (new Expression[] { Expression.Constant (214578) }).AsReadOnly();
      Expect.Call (VisitorMock.VisitAndConvert (elementInit.Arguments, "VisitElementInit")).Return (newArguments);

      var result = (ElementInit) InvokeAndCheckVisitObject ("VisitElementInit", elementInit);
      Assert.That (result, Is.Not.SameAs (elementInit));
      Assert.That (result.AddMethod, Is.SameAs (elementInit.AddMethod));
      Assert.That (result.Arguments, Is.EqualTo (newArguments));
    }

    [Test]
    public void VisitMemberBinding_Delegation_MemberAssignment ()
    {
      MemberAssignment memberAssignment = ExpressionInstanceCreator.CreateMemberAssignment();

      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberAssignment)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);

      Expect.Call (InvokeVisitMethod ("VisitMemberAssignment", memberAssignment)).Return (memberAssignment);

      MockRepository.ReplayAll();
      object result = InvokeVisitMethod ("VisitMemberBinding", memberAssignment);
      MockRepository.VerifyAll();

      Assert.That (result, Is.SameAs (memberAssignment));
    }

    [Test]
    public void VisitMemberBinding_Delegation_MemberBinding ()
    {
      MemberMemberBinding memberMemberBinding = ExpressionInstanceCreator.CreateMemberMemberBinding();

      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberMemberBinding)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);
      Expect.Call (InvokeVisitMethod ("VisitMemberMemberBinding", memberMemberBinding)).Return (memberMemberBinding);

      MockRepository.ReplayAll();
      object result = InvokeVisitMethod ("VisitMemberBinding", memberMemberBinding);
      MockRepository.VerifyAll();

      Assert.That (result, Is.SameAs (memberMemberBinding));
    }

    [Test]
    public void VisitMemberBinding_Delegation_ListBinding ()
    {
      MemberListBinding memberListBinding = ExpressionInstanceCreator.CreateMemberListBinding();

      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberListBinding)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);
      Expect.Call (InvokeVisitMethod ("VisitMemberListBinding", memberListBinding)).Return (memberListBinding);

      MockRepository.ReplayAll();
      object result = InvokeVisitMethod ("VisitMemberBinding", memberListBinding);
      MockRepository.VerifyAll();

      Assert.That (result, Is.SameAs (memberListBinding));
    }

    [Test]
    public void VisitMemberAssignment_Unchanged ()
    {
      MemberAssignment memberAssignment = ExpressionInstanceCreator.CreateMemberAssignment();
      Expect.Call (VisitorMock.VisitExpression (memberAssignment.Expression)).Return (memberAssignment.Expression);
      var result = (MemberAssignment) InvokeAndCheckVisitObject ("VisitMemberAssignment", memberAssignment);
      Assert.That (result, Is.SameAs (memberAssignment));
    }

    [Test]
    public void VisitMemberAssignment_Changed ()
    {
      MemberAssignment memberAssignment = ExpressionInstanceCreator.CreateMemberAssignment ();
      MemberAssignment newMemberAssignment = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("2"));

      Expect.Call (VisitorMock.VisitExpression (memberAssignment.Expression)).Return (newMemberAssignment.Expression);

      var result = (MemberAssignment) InvokeAndCheckVisitObject ("VisitMemberAssignment", memberAssignment);
      Assert.That (result, Is.Not.SameAs (memberAssignment));
    }

    [Test]
    public void VisitMemberMemberBinding_Unchanged ()
    {
      MemberMemberBinding memberMemberBinding = ExpressionInstanceCreator.CreateMemberMemberBinding();
      Expect.Call (InvokeVisitMethod ("VisitMemberBindingList", memberMemberBinding.Bindings)).Return (memberMemberBinding.Bindings);
      var result = (MemberMemberBinding) InvokeAndCheckVisitObject ("VisitMemberMemberBinding", memberMemberBinding);
      Assert.That (result, Is.SameAs (memberMemberBinding));
    }

    [Test]
    public void VisitMemberMemberBinding_Changed ()
    {
      MemberMemberBinding memberMemberBinding = ExpressionInstanceCreator.CreateMemberMemberBinding ();
      var capacityProperty = ((FieldInfo) memberMemberBinding.Member).FieldType.GetProperty ("Capacity");
     
      ReadOnlyCollection<MemberBinding> newBindings = new List<MemberBinding> { Expression.Bind (capacityProperty, Expression.Constant (2765865)) }.AsReadOnly ();
      Expect.Call (InvokeVisitMethod ("VisitMemberBindingList", memberMemberBinding.Bindings)).Return (newBindings);
      var result = (MemberMemberBinding) InvokeAndCheckVisitObject ("VisitMemberMemberBinding", memberMemberBinding);
      Assert.That (result, Is.Not.SameAs (memberMemberBinding));
      Assert.That (result.Bindings, Is.EqualTo (newBindings));
      Assert.That (result.BindingType, Is.EqualTo (memberMemberBinding.BindingType));
      Assert.That (result.Member, Is.EqualTo (memberMemberBinding.Member));
    }

    [Test]
    public void VisitMemberListBinding_Unchanged ()
    {
      MemberListBinding memberListBinding = ExpressionInstanceCreator.CreateMemberListBinding();
      Expect.Call (InvokeVisitMethod ("VisitElementInitList", memberListBinding.Initializers)).Return (memberListBinding.Initializers);
      var result = (MemberListBinding) InvokeAndCheckVisitObject ("VisitMemberListBinding", memberListBinding);
      Assert.That (result, Is.SameAs (memberListBinding));
    }

    [Test]
    public void VisitMemberListBinding_Changed ()
    {
      MemberListBinding memberListBinding = ExpressionInstanceCreator.CreateMemberListBinding();
      var addMethod = ((FieldInfo) memberListBinding.Member).FieldType.GetMethod ("Add");
      ReadOnlyCollection<ElementInit> newInitializers = new List<ElementInit> { Expression.ElementInit(addMethod, Expression.Constant (2765865)) }.AsReadOnly ();
      Expect.Call (InvokeVisitMethod ("VisitElementInitList", memberListBinding.Initializers)).Return (newInitializers);
      var result = (MemberListBinding) InvokeAndCheckVisitObject ("VisitMemberListBinding", memberListBinding);
      Assert.That (result, Is.Not.SameAs (memberListBinding));
      Assert.That (result.Initializers, Is.EqualTo (newInitializers));
      Assert.That (result.BindingType, Is.EqualTo (memberListBinding.BindingType));
      Assert.That (result.Member, Is.EqualTo (memberListBinding.Member));
    }

    

    [Test]
    public void VisitMemberBindingList_Unchanged ()
    {
      MemberBinding memberBinding1 = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("0"));
      MemberBinding memberBinding2 = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("1"));
      ReadOnlyCollection<MemberBinding> memberBindings = new List<MemberBinding> (new[] { memberBinding1, memberBinding2 }).AsReadOnly();
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding1)).Return (memberBinding1);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding2)).Return (memberBinding2);
      ReadOnlyCollection<MemberBinding> result = InvokeAndCheckVisitMemberBindingList (memberBindings);
      Assert.That (result, Is.SameAs (memberBindings));
    }

    [Test]
    public void VisitMemberBindingList_Changed ()
    {
      MemberBinding memberBinding1 = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("0"));
      MemberBinding memberBinding2 = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("1"));
      MemberBinding memberBinding3 = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("2"));
      MemberBinding newMemberBinding = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("3"));
      ReadOnlyCollection<MemberBinding> memberBindings =
          new List<MemberBinding> (new[] { memberBinding1, memberBinding2, memberBinding3 }).AsReadOnly();
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding1)).Return (memberBinding1);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding2)).Return (newMemberBinding);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding3)).Return (memberBinding3);
      ReadOnlyCollection<MemberBinding> result = InvokeAndCheckVisitMemberBindingList (memberBindings);
      Assert.That (result, Is.Not.SameAs (memberBindings));
      Assert.That (result, Is.EqualTo (new object[] { memberBinding1, newMemberBinding, memberBinding3 }));
    }

    [Test]
    public void VisitElementInitList_Unchanged ()
    {
      ElementInit elementInit1 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (0));
      ElementInit elementInit2 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1));
      ReadOnlyCollection<ElementInit> elementInits = new List<ElementInit> (new[] { elementInit1, elementInit2 }).AsReadOnly();
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit1)).Return (elementInit1);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit2)).Return (elementInit2);
      ReadOnlyCollection<ElementInit> result = InvokeAndCheckVisitElementInitList (elementInits);
      Assert.That (result, Is.SameAs (elementInits));
    }

    [Test]
    public void VisitElementInitList_Changed ()
    {
      ElementInit elementInit1 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (0));
      ElementInit elementInit2 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1));
      ElementInit elementInit3 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (2));
      ElementInit newElementInit = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (3));
      ReadOnlyCollection<ElementInit> elementInits = new List<ElementInit> (new[] { elementInit1, elementInit2, elementInit3 }).AsReadOnly();
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit1)).Return (elementInit1);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit2)).Return (newElementInit);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit3)).Return (elementInit3);
      ReadOnlyCollection<ElementInit> result = InvokeAndCheckVisitElementInitList (elementInits);
      Assert.That (result, Is.Not.SameAs (elementInits));
      Assert.That (result, Is.EqualTo (new object[] { elementInit1, newElementInit, elementInit3 }));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Expression type 'SpecialExpressionNode' is not supported by this ExpressionTreeVisitor.*\\.", MatchType = MessageMatch.Regex)]
    public void VisitUnknownNonExtension ()
    {
      var expressionNode = new SpecialExpressionNode ((ExpressionType) (-1), typeof (int));
      Expect.Call (InvokeVisitMethod ("VisitUnknownNonExtension", expressionNode)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);
      MockRepository.ReplayAll();

      try
      {
        InvokeVisitMethod ("VisitUnknownNonExtension", expressionNode);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException;
      }
    }

    [Test]
    public void VisitExtension_CallsVisitChildren ()
    {
      var expectedResult = Expression.Constant (0);

      var extensionExpressionMock = MockRepository.StrictMock<ExtensionExpression> (typeof (int));
      Expect.Call (InvokeVisitMethod ("VisitExtension", extensionExpressionMock)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);
      extensionExpressionMock
          .Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "VisitChildren", VisitorMock))
          .Return (expectedResult);
      MockRepository.ReplayAll();

      var result = InvokeVisitMethod ("VisitExtension", extensionExpressionMock);

      MockRepository.VerifyAll ();
      Assert.That (result, Is.SameAs (expectedResult));
    }
  }
}
