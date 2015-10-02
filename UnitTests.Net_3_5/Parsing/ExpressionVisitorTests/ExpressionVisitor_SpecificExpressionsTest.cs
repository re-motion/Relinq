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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Linq.Clauses.Expressions;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitorTests
{
  [TestFixture]
  public class ExpressionVisitor_SpecificExpressionsTest : ExpressionVisitorTestBase
  {
    [Test]
    public void VisitUnary_Unchanges ()
    {
      var expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.UnaryPlus);
      Expression expectedNextVisit = expression.Operand;
      Expect.Call (VisitorMock.Visit (expectedNextVisit)).Return (expectedNextVisit);

      Assert.That (InvokeAndCheckVisit ("VisitUnary", expression), Is.SameAs (expression));
    }

    [Test]
    public void VisitUnary_UnaryPlus_Changes ()
    {
      var expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.UnaryPlus);
      Expression expectedNextVisit = expression.Operand;
      Expression newOperand = Expression.Constant (1);
      Expect.Call (VisitorMock.Visit (expectedNextVisit)).Return (newOperand);

      var result = (UnaryExpression) InvokeAndCheckVisit ("VisitUnary", expression);
      Assert.That (result.Operand, Is.SameAs (newOperand));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.UnaryPlus));
    }

    [Test]
    public void VisitUnary_Negate_Changes ()
    {
      var expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Negate);
      Expression expectedNextVisit = expression.Operand;
      Expression newOperand = Expression.Constant (1);
      Expect.Call (VisitorMock.Visit (expectedNextVisit)).Return (newOperand);

      var result = (UnaryExpression) InvokeAndCheckVisit ("VisitUnary", expression);
      Assert.That (result.Operand, Is.SameAs (newOperand));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Negate));
    }

    [Test]
    public void VisitTypeBinary_Unchanged ()
    {
      var expression = (TypeBinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.TypeIs);
      Expect.Call (VisitorMock.Visit (expression.Expression)).Return (expression.Expression);
      var result = (TypeBinaryExpression) InvokeAndCheckVisit ("VisitTypeBinary", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitTypeBinary_Changed ()
    {
      var expression = (TypeBinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.TypeIs);
      Expression newExpression = Expression.Constant (1);
      Expect.Call (VisitorMock.Visit (expression.Expression)).Return (newExpression);
      var result = (TypeBinaryExpression) InvokeAndCheckVisit ("VisitTypeBinary", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.TypeIs));
      Assert.That (result.Expression, Is.SameAs (newExpression));
    }

    [Test]
    public void VisitConstant ()
    {
      var expression = (ConstantExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Constant);
      var result = (ConstantExpression) InvokeAndCheckVisit ("VisitConstant", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitConditional_Unchanged ()
    {
      var expression = (ConditionalExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Conditional);
      Expect.Call (VisitorMock.Visit (expression.Test)).Return (expression.Test);
      Expect.Call (VisitorMock.Visit (expression.IfTrue)).Return (expression.IfTrue);
      Expect.Call (VisitorMock.Visit (expression.IfFalse)).Return (expression.IfFalse);
      var result = (ConditionalExpression) InvokeAndCheckVisit ("VisitConditional", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitConditional_ChangedTest ()
    {
      var expression = (ConditionalExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Conditional);
      Expression newTest = Expression.Constant (true);
      Expect.Call (VisitorMock.Visit (expression.Test)).Return (newTest);
      Expect.Call (VisitorMock.Visit (expression.IfTrue)).Return (expression.IfTrue);
      Expect.Call (VisitorMock.Visit (expression.IfFalse)).Return (expression.IfFalse);
      var result = (ConditionalExpression) InvokeAndCheckVisit ("VisitConditional", expression);
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
      Expect.Call (VisitorMock.Visit (expression.Test)).Return (expression.Test);
      Expect.Call (VisitorMock.Visit (expression.IfTrue)).Return (expression.IfTrue);
      Expect.Call (VisitorMock.Visit (expression.IfFalse)).Return (newFalse);
      var result = (ConditionalExpression) InvokeAndCheckVisit ("VisitConditional", expression);
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
      Expect.Call (VisitorMock.Visit (expression.Test)).Return (expression.Test);
      Expect.Call (VisitorMock.Visit (expression.IfTrue)).Return (newTrue);
      Expect.Call (VisitorMock.Visit (expression.IfFalse)).Return (expression.IfFalse);
      var result = (ConditionalExpression) InvokeAndCheckVisit ("VisitConditional", expression);
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
      VisitorMock.Expect (mock => mock.Visit (expression.Test)).Return (expression.Test);
      VisitorMock.Expect (mock => mock.Visit (expression.IfTrue)).Return (expression.IfTrue);
      VisitorMock.Expect (mock => mock.Visit (expression.IfFalse)).Return (newIfFalse);

      var result = (ConditionalExpression) InvokeAndCheckVisit ("VisitConditional", expression);

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
      var result = (ParameterExpression) InvokeAndCheckVisit ("VisitParameter", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitLambda_Unchanged ()
    {
      var expression = ExpressionInstanceCreator.CreateLambdaWithArguments ();
      var parameter = expression.Parameters.Single();
      Expect.Call (VisitorMock.Visit (expression.Body)).Return (expression.Body);
      Expect.Call (VisitorMock.Visit (parameter)).Return (parameter);
      var result = (LambdaExpression) InvokeAndCheckVisit ("VisitLambda", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitLambda_ChangedBody ()
    {
      var expression = ExpressionInstanceCreator.CreateLambdaWithArguments ();
      var parameter = expression.Parameters.Single();
      Expression newBody = Expression.Constant (1);
      Expect.Call (VisitorMock.Visit (expression.Body)).Return (newBody);
      Expect.Call (VisitorMock.Visit (parameter)).Return (parameter);
      var result = (LambdaExpression) InvokeAndCheckVisit ("VisitLambda", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Lambda));
      Assert.That (result.Body, Is.SameAs (newBody));
      Assert.That (result.Parameters, Is.SameAs (expression.Parameters));
    }

    [Test]
    public void VisitLambda_ChangedParameters ()
    {
      var expression = ExpressionInstanceCreator.CreateLambdaWithArguments ();
      var parameter = expression.Parameters.Single();
      Expression newParameter = Expression.Parameter (typeof (int), "i");
      Expect.Call (VisitorMock.Visit (expression.Body)).Return (expression.Body);
      Expect.Call (VisitorMock.Visit (parameter)).Return (newParameter);
      var result = (LambdaExpression) InvokeAndCheckVisit ("VisitLambda", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Lambda));
      Assert.That (result.Parameters, Is.EqualTo (new[] { newParameter }));
      Assert.That (result.Body, Is.SameAs (expression.Body));
    }

    [Test]
    public void VisitMethodCall_Unchanged ()
    {
      var expression = (MethodCallExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Call);
      var argument = expression.Arguments.Single();
      Expect.Call (VisitorMock.Visit (expression.Object)).Return (expression.Object);
      Expect.Call (VisitorMock.Visit (argument)).Return (argument);
      var result = (MethodCallExpression) InvokeAndCheckVisit ("VisitMethodCall", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitMethodCall_ChangedObject ()
    {
      var expression = (MethodCallExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Call);
      var argument = expression.Arguments.Single();
      Expression newObject = Expression.Constant (1);
      Expect.Call (VisitorMock.Visit (expression.Object)).Return (newObject);
      Expect.Call (VisitorMock.Visit (argument)).Return (argument);
      var result = (MethodCallExpression) InvokeAndCheckVisit ("VisitMethodCall", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Call));
      Assert.That (result.Object, Is.SameAs (newObject));
      Assert.That (result.Arguments, Is.SameAs (expression.Arguments));
    }

    [Test]
    public void VisitMethodCall_ChangedArguments ()
    {
      var expression = (MethodCallExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Call);
      var argument = expression.Arguments.Single();
      Expression newArgument = Expression.Constant (214578);
      Expect.Call (VisitorMock.Visit (expression.Object)).Return (expression.Object);
      Expect.Call (VisitorMock.Visit (argument)).Return (newArgument);
      var result = (MethodCallExpression) InvokeAndCheckVisit ("VisitMethodCall", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Call));
      Assert.That (result.Arguments, Is.EqualTo (new[] { newArgument }));
      Assert.That (result.Object, Is.SameAs (expression.Object));
    }

    [Test]
    public void VisitInvocation_Unchanged ()
    {
      var expression = ExpressionInstanceCreator.CreateInvokeWithArguments ();
      var argument = expression.Arguments.Single();
      Expect.Call (VisitorMock.Visit (expression.Expression)).Return (expression.Expression);
      Expect.Call (VisitorMock.Visit (argument)).Return (argument);
      var result = (InvocationExpression) InvokeAndCheckVisit ("VisitInvocation", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitInvocation_ChangedObject ()
    {
      var expression = ExpressionInstanceCreator.CreateInvokeWithArguments ();
      var argument = expression.Arguments.Single();
      Expression newExpression = Expression.Lambda (Expression.Constant (1), Expression.Parameter (typeof (int), "i"));
      Expect.Call (VisitorMock.Visit (expression.Expression)).Return (newExpression);
      Expect.Call (VisitorMock.Visit (argument)).Return (argument);
      var result = (InvocationExpression) InvokeAndCheckVisit ("VisitInvocation", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Invoke));
      Assert.That (result.Expression, Is.SameAs (newExpression));
      Assert.That (result.Arguments, Is.SameAs (expression.Arguments));
    }

    [Test]
    public void VisitInvocation_ChangedArguments ()
    {
      var expression = ExpressionInstanceCreator.CreateInvokeWithArguments ();
      var argument = expression.Arguments.Single();
      Expression newArgument = Expression.Constant (214578);
      Expect.Call (VisitorMock.Visit (expression.Expression)).Return (expression.Expression);
      Expect.Call (VisitorMock.Visit (argument)).Return (newArgument);
      var result = (InvocationExpression) InvokeAndCheckVisit ("VisitInvocation", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.Invoke));
      Assert.That (result.Arguments, Is.EqualTo (new[] { newArgument }));
      Assert.That (result.Expression, Is.SameAs (expression.Expression));
    }

    [Test]
    public void VisitMember_Unchanged ()
    {
      var expression = (MemberExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberAccess);
      Expect.Call (VisitorMock.Visit (expression.Expression)).Return (expression.Expression);
      var result = (MemberExpression) InvokeAndCheckVisit ("VisitMember", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitMember_ChangedExpression ()
    {
      var expression = (MemberExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberAccess);
      Expression newExpression = Expression.Constant (DateTime.Now);
      Expect.Call (VisitorMock.Visit (expression.Expression)).Return (newExpression);
      var result = (MemberExpression) InvokeAndCheckVisit ("VisitMember", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.MemberAccess));
      Assert.That (result.Expression, Is.SameAs (newExpression));
    }

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
    public void VisitNewArray_Unchanged ()
    {
      var expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayInit);
      var initExpression = expression.Expressions.Single();
      Expect.Call (VisitorMock.Visit (initExpression)).Return (initExpression);
      var result = (NewArrayExpression) InvokeAndCheckVisit ("VisitNewArray", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNewArray_Init_Changed ()
    {
      var expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayInit);
      var initExpression = expression.Expressions.Single();
      var newInitExpression = Expression.Constant (214578);
      Expect.Call (VisitorMock.Visit (initExpression)).Return (newInitExpression);
      var result = (NewArrayExpression) InvokeAndCheckVisit ("VisitNewArray", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.NewArrayInit));
      Assert.That (result.Expressions, Is.EqualTo (new[] { newInitExpression }));
      Assert.That (result.Type, Is.EqualTo (typeof (int[])));
    }

    [Test]
    public void VisitNewArray_Bounds_Changed ()
    {
      var expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayBounds);
      var initExpression = expression.Expressions.Single();
      var newInitExpression = Expression.Constant (214578);
      Expect.Call (VisitorMock.Visit (initExpression)).Return (newInitExpression);
      var result = (NewArrayExpression) InvokeAndCheckVisit ("VisitNewArray", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.NewArrayBounds));
      Assert.That (result.Expressions, Is.EqualTo (new[]{newInitExpression}));
#if !NET_3_5
      Assert.That (result.Type, Is.EqualTo (typeof (int).MakeArrayType()));
#else
      Assert.That (result.Type, Is.EqualTo (typeof (int).MakeArrayType (1)));
#endif
    }

    [Test]
    public void VisitMemberInit_Unchanged ()
    {
      var expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      var binding = expression.Bindings.Single();
      Expect.Call (VisitorMock.Visit (expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", binding)).Return (binding);
      var result = (MemberInitExpression) InvokeAndCheckVisit ("VisitMemberInit", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
#if !NET_3_5
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = 
        "When called from 'VisitMemberInit', rewriting a node of type 'System.Linq.Expressions.NewExpression' must return a non-null value of the same type. "
        + "Alternatively, override 'VisitMemberInit' and change it to not visit children of this type.")]
#else
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "MemberInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.")]
#endif
    public void VisitMemberInit_InvalidNewExpression ()
    {
      var expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      var binding = expression.Bindings.Single();
      Expect.Call (VisitorMock.Visit (expression.NewExpression)).Return (Expression.Constant (0));
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", binding)).Return (binding);
      try
      {
        InvokeAndCheckVisit ("VisitMemberInit", expression);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException;
      }
    }

    [Test]
    public void VisitMemberInit_ChangedNewExpression ()
    {
      var expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      var binding = expression.Bindings.Single();
      NewExpression newNewExpression = Expression.New (typeof (List<int>));
      Expect.Call (VisitorMock.Visit (expression.NewExpression)).Return (newNewExpression);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", binding)).Return (binding);
      var result = (MemberInitExpression) InvokeAndCheckVisit ("VisitMemberInit", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.MemberInit));
      Assert.That (result.NewExpression, Is.SameAs (newNewExpression));
      Assert.That (result.Bindings, Is.SameAs (expression.Bindings));
    }

    [Test]
    public void VisitMemberInit_ChangedBindings ()
    {
      var expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      var binding = expression.Bindings.Single();
      var capacityProperty = expression.NewExpression.Constructor.DeclaringType.GetProperty ("Capacity");

      MemberBinding newBinding = Expression.Bind (capacityProperty, Expression.Constant (214578)) ;
      Expect.Call (VisitorMock.Visit (expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", binding)).Return (newBinding);
      var result = (MemberInitExpression) InvokeAndCheckVisit ("VisitMemberInit", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.MemberInit));
      Assert.That (result.Bindings, Is.EqualTo (new[] { newBinding }));
      Assert.That (result.NewExpression, Is.SameAs (expression.NewExpression));
    }

    [Test]
    public void VisitListInit_Unchanged ()
    {
      var expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      var elementInit = expression.Initializers.Single();
      Expect.Call (VisitorMock.Visit (expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit)).Return (elementInit);
      var result = (ListInitExpression) InvokeAndCheckVisit ("VisitListInit", expression);
      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
#if !NET_3_5
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = 
        "When called from 'VisitListInit', rewriting a node of type 'System.Linq.Expressions.NewExpression' must return a non-null value of the same type. "
        + "Alternatively, override 'VisitListInit' and change it to not visit children of this type.")]
#else
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "ListInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.")]
#endif
    public void VisitListInit_InvalidNewExpression ()
    {
      var expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      var elementInit = expression.Initializers.Single();
      Expect.Call (VisitorMock.Visit (expression.NewExpression)).Return (Expression.Constant (0));
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit)).Return (elementInit);
      try
      {
        InvokeAndCheckVisit ("VisitListInit", expression);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException;
      }
    }

    [Test]
    public void VisitListInit_ChangedNewExpression ()
    {
      var expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      var initializer = expression.Initializers.Single();
      NewExpression newNewExpression = Expression.New (typeof (List<int>));
      Expect.Call (VisitorMock.Visit (expression.NewExpression)).Return (newNewExpression);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", initializer)).Return (initializer);
      var result = (ListInitExpression) InvokeAndCheckVisit ("VisitListInit", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.ListInit));
      Assert.That (result.NewExpression, Is.SameAs (newNewExpression));
      Assert.That (result.Initializers, Is.SameAs (expression.Initializers));
    }

    [Test]
    public void VisitListInit_ChangedInitializers ()
    {
      var expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      var initializer = expression.Initializers.Single();
      var newInitializer = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (214578));
      Expect.Call (VisitorMock.Visit (expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", initializer)).Return (newInitializer);
      var result = (ListInitExpression) InvokeAndCheckVisit ("VisitListInit", expression);
      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.ListInit));
      Assert.That (result.Initializers, Is.EqualTo (new[] { newInitializer }));
      Assert.That (result.NewExpression, Is.SameAs (expression.NewExpression));
    }

    [Test]
    public void VisitElementInit_Unchanged ()
    {
      ElementInit elementInit = ExpressionInstanceCreator.CreateElementInit();
      var argument = elementInit.Arguments.Single();
      Expect.Call (VisitorMock.Visit (argument)).Return (argument);

      var result = (ElementInit) InvokeAndCheckVisitObject ("VisitElementInit", elementInit);
      Assert.That (result, Is.SameAs (elementInit));
    }

    [Test]
    public void VisitElementInit_Changed ()
    {
      ElementInit elementInit = ExpressionInstanceCreator.CreateElementInit ();
      var argument = elementInit.Arguments.Single();
      Expression newArgument = Expression.Constant (214578);
      Expect.Call (VisitorMock.Visit (argument)).Return (newArgument);

      var result = (ElementInit) InvokeAndCheckVisitObject ("VisitElementInit", elementInit);
      Assert.That (result, Is.Not.SameAs (elementInit));
      Assert.That (result.AddMethod, Is.SameAs (elementInit.AddMethod));
      Assert.That (result.Arguments, Is.EqualTo (new[] { newArgument }));
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
      MemberMemberBinding memberMemberBinding = ExpressionInstanceCreator.CreateMemberMemberBinding (new MemberBinding[0]);

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
      MemberListBinding memberListBinding = ExpressionInstanceCreator.CreateMemberListBinding (new ElementInit[0]);

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
      Expect.Call (VisitorMock.Visit (memberAssignment.Expression)).Return (memberAssignment.Expression);
      var result = (MemberAssignment) InvokeAndCheckVisitObject ("VisitMemberAssignment", memberAssignment);
      Assert.That (result, Is.SameAs (memberAssignment));
    }

    [Test]
    public void VisitMemberAssignment_Changed ()
    {
      MemberAssignment memberAssignment = ExpressionInstanceCreator.CreateMemberAssignment ();
      MemberAssignment newMemberAssignment = Expression.Bind (typeof (List<int>).GetProperty ("Capacity"), Expression.Constant (2));

      Expect.Call (VisitorMock.Visit (memberAssignment.Expression)).Return (newMemberAssignment.Expression);

      var result = (MemberAssignment) InvokeAndCheckVisitObject ("VisitMemberAssignment", memberAssignment);
      Assert.That (result, Is.Not.SameAs (memberAssignment));
    }

    [Test]
    public void VisitMemberMemberBinding_Unchanged ()
    {
      MemberBinding memberBinding1 = Expression.Bind (typeof (List<int>).GetProperty ("Capacity"), Expression.Constant (0));
      MemberBinding memberBinding2 = Expression.Bind (typeof (List<int>).GetProperty ("Capacity"), Expression.Constant (1));
      MemberMemberBinding memberMemberBinding = ExpressionInstanceCreator.CreateMemberMemberBinding(new[] { memberBinding1, memberBinding2 });
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding1)).Return (memberBinding1);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding2)).Return (memberBinding2);
      var result = (MemberMemberBinding) InvokeAndCheckVisitObject ("VisitMemberMemberBinding", memberMemberBinding);
      Assert.That (result, Is.SameAs (memberMemberBinding));
    }

    [Test]
    public void VisitMemberMemberBinding_Changed ()
    {
      MemberBinding memberBinding1 = Expression.Bind (typeof (List<int>).GetProperty ("Capacity"), Expression.Constant (0));
      MemberBinding memberBinding2 = Expression.Bind (typeof (List<int>).GetProperty ("Capacity"), Expression.Constant (1));
      MemberBinding memberBinding3 = Expression.Bind (typeof (List<int>).GetProperty ("Capacity"), Expression.Constant (2));
      MemberBinding newMemberBinding = Expression.Bind (typeof (List<int>).GetProperty ("Capacity"), Expression.Constant (3));
      MemberMemberBinding memberMemberBinding =
          ExpressionInstanceCreator.CreateMemberMemberBinding (new[] { memberBinding1, memberBinding2, memberBinding3 });

      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding1)).Return (memberBinding1);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding2)).Return (newMemberBinding);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding3)).Return (memberBinding3);
      var result = (MemberMemberBinding) InvokeAndCheckVisitObject ("VisitMemberMemberBinding", memberMemberBinding);
      Assert.That (result, Is.Not.SameAs (memberMemberBinding));
      Assert.That (result.Bindings, Is.EqualTo (new[] { memberBinding1, newMemberBinding, memberBinding3 }));
      Assert.That (result.BindingType, Is.EqualTo (memberMemberBinding.BindingType));
      Assert.That (result.Member, Is.EqualTo (memberMemberBinding.Member));
    }

    [Test]
    public void VisitMemberListBinding_Unchanged ()
    {
      ElementInit elementInit1 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (0));
      ElementInit elementInit2 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1));
      ReadOnlyCollection<ElementInit> elementInits = new List<ElementInit> (new[] { elementInit1, elementInit2 }).AsReadOnly();
      MemberListBinding memberListBinding = ExpressionInstanceCreator.CreateMemberListBinding (elementInits);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit1)).Return (elementInit1);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit2)).Return (elementInit2);
      var result = (MemberListBinding) InvokeAndCheckVisitObject ("VisitMemberListBinding", memberListBinding);
      Assert.That (result, Is.SameAs (memberListBinding));
    }

    [Test]
    public void VisitMemberListBinding_Changed ()
    {
      ElementInit elementInit1 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (0));
      ElementInit elementInit2 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1));
      ElementInit elementInit3 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (2));
      ElementInit newElementInit = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (3));
      ReadOnlyCollection<ElementInit> elementInits = new List<ElementInit> (new[] { elementInit1, elementInit2, elementInit3 }).AsReadOnly();
      MemberListBinding memberListBinding = ExpressionInstanceCreator.CreateMemberListBinding (elementInits);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit1)).Return (elementInit1);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit2)).Return (newElementInit);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit3)).Return (elementInit3);
      var result = (MemberListBinding) InvokeAndCheckVisitObject ("VisitMemberListBinding", memberListBinding);
      Assert.That (result, Is.Not.SameAs (memberListBinding));
      Assert.That (result.Initializers, Is.EqualTo (new List<ElementInit> { elementInit1, newElementInit, elementInit3 }));
      Assert.That (result.BindingType, Is.EqualTo (memberListBinding.BindingType));
      Assert.That (result.Member, Is.EqualTo (memberListBinding.Member));
    }

#if NET_3_5
    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Expression type 'SpecialExpressionNode' is not supported by this ExpressionVisitor.*\\.", MatchType = MessageMatch.Regex)]
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
#endif

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
