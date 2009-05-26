// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using System.Linq.Expressions;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using System.Reflection;
using Rhino.Mocks.Interfaces;

namespace Remotion.Data.UnitTests.Linq.Visitor.ExpressionTreeVisitorTests
{
  [TestFixture]
  public class ExpressionTreeVisitor_SpecificExpressionsTest : ExpressionTreeVisitor_SpecificExpressionsTestBase
  {
    [Test]
    public void VisitUnaryExpression_Unchanges ()
    {
      UnaryExpression expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.UnaryPlus);
      Expression expectedNextVisit = expression.Operand;
      Expect.Call (InvokeVisitMethod ("VisitExpression", expectedNextVisit)).Return (expectedNextVisit);

      Assert.AreSame (expression, InvokeAndCheckVisitExpression ("VisitUnaryExpression", expression));
    }

    [Test]
    public void VisitUnaryExpression_UnaryPlus_Changes ()
    {
      UnaryExpression expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.UnaryPlus);
      Expression expectedNextVisit = expression.Operand;
      Expression newOperand = Expression.Constant (1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expectedNextVisit)).Return (newOperand);

      UnaryExpression result = (UnaryExpression) InvokeAndCheckVisitExpression ("VisitUnaryExpression", expression);
      Assert.AreSame (newOperand, result.Operand);
      Assert.AreEqual (ExpressionType.UnaryPlus, result.NodeType);
    }

    [Test]
    public void VisitUnaryExpression_Negate_Changes ()
    {
      UnaryExpression expression = (UnaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Negate);
      Expression expectedNextVisit = expression.Operand;
      Expression newOperand = Expression.Constant (1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expectedNextVisit)).Return (newOperand);

      UnaryExpression result = (UnaryExpression) InvokeAndCheckVisitExpression ("VisitUnaryExpression", expression);
      Assert.AreSame (newOperand, result.Operand);
      Assert.AreEqual (ExpressionType.Negate, result.NodeType);
    }

    [Test]
    public void VisitTypeBinary_Unchanged()
    {

      TypeBinaryExpression expression = (TypeBinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.TypeIs); 
       Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Expression)).Return (expression.Expression);
      TypeBinaryExpression result = (TypeBinaryExpression) InvokeAndCheckVisitExpression ("VisitTypeBinaryExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitTypeBinary_Changed ()
    {

      TypeBinaryExpression expression = (TypeBinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.TypeIs);
      Expression newExpression = Expression.Constant (1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Expression)).Return (newExpression);
      TypeBinaryExpression result = (TypeBinaryExpression) InvokeAndCheckVisitExpression ("VisitTypeBinaryExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.TypeIs, result.NodeType);
      Assert.AreSame (newExpression, result.Expression);
    }

    [Test]
    public void VisitConstantExpression()
    {
      ConstantExpression expression = (ConstantExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Constant);
      ConstantExpression result = (ConstantExpression) InvokeAndCheckVisitExpression ("VisitConstantExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitConditionalExpression_Unchanged()
    {
      ConditionalExpression expression = (ConditionalExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Conditional);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Test)).Return (expression.Test);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.IfFalse)).Return (expression.IfFalse);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.IfTrue)).Return (expression.IfTrue);
      ConditionalExpression result = (ConditionalExpression) InvokeAndCheckVisitExpression ("VisitConditionalExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitConditionalExpression_ChangedTest ()
    {
      ConditionalExpression expression = (ConditionalExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Conditional);
      Expression newTest = Expression.Constant (true);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Test)).Return (newTest);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.IfFalse)).Return (expression.IfFalse);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.IfTrue)).Return (expression.IfTrue);
      ConditionalExpression result = (ConditionalExpression) InvokeAndCheckVisitExpression ("VisitConditionalExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Conditional, result.NodeType);
      Assert.AreSame (newTest, result.Test);
      Assert.AreSame (expression.IfFalse, result.IfFalse);
      Assert.AreSame (expression.IfTrue, result.IfTrue);
    }

    [Test]
    public void VisitConditionalExpression_ChangedFalse ()
    {
      ConditionalExpression expression = (ConditionalExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Conditional);
      Expression newFalse = Expression.Constant (1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Test)).Return (expression.Test);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.IfFalse)).Return (newFalse);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.IfTrue)).Return (expression.IfTrue);
      ConditionalExpression result = (ConditionalExpression) InvokeAndCheckVisitExpression ("VisitConditionalExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Conditional, result.NodeType);
      Assert.AreSame (newFalse, result.IfFalse);
      Assert.AreSame (expression.Test, result.Test);
      Assert.AreSame (expression.IfTrue, result.IfTrue);
    }

    [Test]
    public void VisitConditionalExpression_ChangedTrue ()
    {
      ConditionalExpression expression = (ConditionalExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Conditional);
      Expression newTrue = Expression.Constant (1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Test)).Return (expression.Test);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.IfFalse)).Return (expression.IfFalse);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.IfTrue)).Return (newTrue);
      ConditionalExpression result = (ConditionalExpression) InvokeAndCheckVisitExpression ("VisitConditionalExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Conditional, result.NodeType);
      Assert.AreSame (newTrue, result.IfTrue);
      Assert.AreSame (expression.Test, result.Test);
      Assert.AreSame (expression.IfFalse, result.IfFalse);
    }

    [Test]
    public void VisitParameterExpression()
    {
      ParameterExpression expression = (ParameterExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Parameter);
      ParameterExpression result = (ParameterExpression) InvokeAndCheckVisitExpression ("VisitParameterExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitLambdaExpression_Unchanged()
    {
      LambdaExpression expression = (LambdaExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Lambda);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Body)).Return (expression.Body);
      Expect.Call (InvokeVisitExpressionListMethod (expression.Parameters)).Return (expression.Parameters);
      LambdaExpression result = (LambdaExpression) InvokeAndCheckVisitExpression ("VisitLambdaExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitLambdaExpression_ChangedBody ()
    {
      LambdaExpression expression = (LambdaExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Lambda);
      Expression newBody = Expression.Constant (1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Body)).Return (newBody);
      Expect.Call (InvokeVisitExpressionListMethod (expression.Parameters)).Return (expression.Parameters);
      LambdaExpression result = (LambdaExpression) InvokeAndCheckVisitExpression ("VisitLambdaExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Lambda, result.NodeType);
      Assert.AreSame (newBody, result.Body);
      Assert.AreSame (expression.Parameters, result.Parameters);
    }

    [Test]
    public void VisitLambdaExpression_ChangedParameters ()
    {
      LambdaExpression expression = (LambdaExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Lambda);
      ReadOnlyCollection<ParameterExpression> newParameters = new List<ParameterExpression>().AsReadOnly(); 
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Body)).Return (expression.Body);
      Expect.Call (InvokeVisitExpressionListMethod (expression.Parameters)).Return (newParameters);
      LambdaExpression result = (LambdaExpression) InvokeAndCheckVisitExpression ("VisitLambdaExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Lambda, result.NodeType);
      Assert.AreSame (newParameters, result.Parameters);
      Assert.AreSame (expression.Body, result.Body);
    }

    [Test]
    public void VisitMethodCallExpression_Unchanged()
    {
      MethodCallExpression expression = (MethodCallExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Call);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Object)).Return (expression.Object);
      Expect.Call (InvokeVisitExpressionListMethod (expression.Arguments)).Return (expression.Arguments);
      MethodCallExpression result = (MethodCallExpression) InvokeAndCheckVisitExpression ("VisitMethodCallExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitMethodCallExpression_ChangedObject ()
    {
      MethodCallExpression expression = (MethodCallExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Call);
      Expression newObject = Expression.Constant (1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Object)).Return (newObject);
      Expect.Call (InvokeVisitExpressionListMethod (expression.Arguments)).Return (expression.Arguments);
      MethodCallExpression result = (MethodCallExpression) InvokeAndCheckVisitExpression ("VisitMethodCallExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Call, result.NodeType);
      Assert.AreSame (newObject, result.Object);
      Assert.AreSame (expression.Arguments, result.Arguments);
    }

    [Test]
    public void VisitMethodCallExpression_ChangedArguments ()
    {
      MethodCallExpression expression = (MethodCallExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Call);
      ReadOnlyCollection<Expression> newParameters = new List<Expression> ().AsReadOnly (); 
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Object)).Return (expression.Object);
      Expect.Call (InvokeVisitExpressionListMethod (expression.Arguments)).Return (newParameters);
      MethodCallExpression result = (MethodCallExpression) InvokeAndCheckVisitExpression ("VisitMethodCallExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Call, result.NodeType);
      Assert.AreSame (newParameters, result.Arguments);
      Assert.AreSame (expression.Object, result.Object);
    }

    [Test]
    public void VisitInvocationExpression_Unchanged ()
    {
      InvocationExpression expression = (InvocationExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Invoke);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Expression)).Return (expression.Expression);
      Expect.Call (InvokeVisitExpressionListMethod (expression.Arguments)).Return (expression.Arguments);
      InvocationExpression result = (InvocationExpression) InvokeAndCheckVisitExpression ("VisitInvocationExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitInvocationExpression_ChangedObject ()
    {
      InvocationExpression expression = (InvocationExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Invoke);
      Expression newExpression = Expression.Lambda (Expression.Constant (1));
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Expression)).Return (newExpression);
      Expect.Call (InvokeVisitExpressionListMethod (expression.Arguments)).Return (expression.Arguments);
      InvocationExpression result = (InvocationExpression) InvokeAndCheckVisitExpression ("VisitInvocationExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Invoke, result.NodeType);
      Assert.AreSame (newExpression, result.Expression);
      Assert.AreSame (expression.Arguments, result.Arguments);
    }

    [Test]
    public void VisitInvocationExpression_ChangedArguments ()
    {
      InvocationExpression expression = (InvocationExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Invoke);
      ReadOnlyCollection<Expression> newParameters = new List<Expression> ().AsReadOnly ();
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Expression)).Return (expression.Expression);
      Expect.Call (InvokeVisitExpressionListMethod (expression.Arguments)).Return (newParameters);
      InvocationExpression result = (InvocationExpression) InvokeAndCheckVisitExpression ("VisitInvocationExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Invoke, result.NodeType);
      Assert.AreSame (newParameters, result.Arguments);
      Assert.AreSame (expression.Expression, result.Expression);
    }

    [Test]
    public void VisitMemberExpression_Unchanged ()
    {
      MemberExpression expression = (MemberExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberAccess);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Expression)).Return (expression.Expression);
      MemberExpression result = (MemberExpression) InvokeAndCheckVisitExpression ("VisitMemberExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitMemberExpression_ChangedExpression ()
    {
      MemberExpression expression = (MemberExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberAccess);
      Expression newExpression = Expression.Constant (DateTime.Now);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Expression)).Return (newExpression);
      MemberExpression result = (MemberExpression) InvokeAndCheckVisitExpression ("VisitMemberExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.MemberAccess, result.NodeType);
      Assert.AreSame (newExpression, result.Expression);
    }

    [Test]
    public void VisitNewExpression_Unchanged ()
    {
      NewExpression expression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);
      Expect.Call (InvokeVisitExpressionListMethod (expression.Arguments)).Return (expression.Arguments);
      NewExpression result = (NewExpression) InvokeAndCheckVisitExpression ("VisitNewExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitNewExpression_ChangedArguments ()
    {
      NewExpression expression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);
      ReadOnlyCollection<Expression> newArguments = new List<Expression> ().AsReadOnly ();
      Expect.Call (InvokeVisitExpressionListMethod (expression.Arguments)).Return (newArguments);
      NewExpression result = (NewExpression) InvokeAndCheckVisitExpression ("VisitNewExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.New, result.NodeType);
      Assert.AreSame (newArguments, result.Arguments);
    }

    [Test]
    public void VisitNewArrayExpression_Unchanged ()
    {
      NewArrayExpression expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayInit);
      Expect.Call (InvokeVisitExpressionListMethod (expression.Expressions)).Return (expression.Expressions);
      NewArrayExpression result = (NewArrayExpression) InvokeAndCheckVisitExpression ("VisitNewArrayExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitNewArrayInitExpression_Changed ()
    {
      NewArrayExpression expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayInit);
      ReadOnlyCollection<Expression> newExpressions = new List<Expression> ().AsReadOnly ();
      Expect.Call (InvokeVisitExpressionListMethod (expression.Expressions)).Return (newExpressions);
      NewArrayExpression result = (NewArrayExpression) InvokeAndCheckVisitExpression ("VisitNewArrayExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.NewArrayInit, result.NodeType);
      Assert.AreSame (newExpressions, result.Expressions);
    }

    [Test]
    public void VisitNewArrayBoundsExpression_Changed ()
    {
      NewArrayExpression expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayBounds);
      ReadOnlyCollection<Expression> newExpressions = new List<Expression> (new Expression[] {Expression.Constant(0)}).AsReadOnly ();
      Expect.Call (InvokeVisitExpressionListMethod (expression.Expressions)).Return (newExpressions);
      NewArrayExpression result = (NewArrayExpression) InvokeAndCheckVisitExpression ("VisitNewArrayExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.NewArrayBounds, result.NodeType);
      Assert.AreSame (newExpressions, result.Expressions);
    }

    [Test]
    public void VisitMemberInitExpression_Unchanged ()
    {
      MemberInitExpression expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitMemberBindingList", expression.Bindings)).Return (expression.Bindings);
      MemberInitExpression result = (MemberInitExpression) InvokeAndCheckVisitExpression ("VisitMemberInitExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "MemberInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.")]
    public void VisitMemberInitExpression_InvalidNewExpression ()
    {
      MemberInitExpression expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      Expect.Call (InvokeVisitMethod<NewExpression, ConstantExpression> ("VisitExpression", expression.NewExpression)).Return (Expression.Constant (0));
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
      MemberInitExpression expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      NewExpression newNewExpression = Expression.New (typeof (List<int>));
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.NewExpression)).Return (newNewExpression);
      Expect.Call (InvokeVisitMethod ("VisitMemberBindingList", expression.Bindings)).Return (expression.Bindings);
      MemberInitExpression result = (MemberInitExpression) InvokeAndCheckVisitExpression ("VisitMemberInitExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.MemberInit, result.NodeType);
      Assert.AreSame (newNewExpression, result.NewExpression);
      Assert.AreSame (expression.Bindings, result.Bindings);
    }

    [Test]
    public void VisitMemberInitExpression_ChangedBindings ()
    {
      MemberInitExpression expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      ReadOnlyCollection<MemberBinding> newBindings = new List<MemberBinding>().AsReadOnly();
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitMemberBindingList", expression.Bindings)).Return (newBindings);
      MemberInitExpression result = (MemberInitExpression) InvokeAndCheckVisitExpression ("VisitMemberInitExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.MemberInit, result.NodeType);
      Assert.AreSame (newBindings, result.Bindings);
      Assert.AreSame (expression.NewExpression, result.NewExpression);
    }

    [Test]
    public void VisitListInitExpression_Unchanged ()
    {
      ListInitExpression expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitElementInitList", expression.Initializers)).Return (expression.Initializers);
      ListInitExpression result = (ListInitExpression) InvokeAndCheckVisitExpression ("VisitListInitExpression", expression);
      Assert.AreSame (expression, result);
    }
    
    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "ListInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.")]
    public void VisitListInitExpression_InvalidNewExpression ()
    {
      ListInitExpression expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      Expect.Call (InvokeVisitMethod<NewExpression, ConstantExpression> ("VisitExpression", expression.NewExpression)).Return (Expression.Constant (0));
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
      ListInitExpression expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      NewExpression newNewExpression = Expression.New (typeof (List<int>));
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.NewExpression)).Return (newNewExpression);
      Expect.Call (InvokeVisitMethod ("VisitElementInitList", expression.Initializers)).Return (expression.Initializers);
      ListInitExpression result = (ListInitExpression) InvokeAndCheckVisitExpression ("VisitListInitExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.ListInit, result.NodeType);
      Assert.AreSame (newNewExpression, result.NewExpression);
      Assert.AreSame (expression.Initializers, result.Initializers);
    }

    [Test]
    public void VisitListInitExpression_ChangedInitializers ()
    {
      ListInitExpression expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      ReadOnlyCollection<ElementInit> newInitializers =
          new List<ElementInit> (new [] {Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1))}).AsReadOnly ();
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitMethod ("VisitElementInitList", expression.Initializers)).Return (newInitializers);
      ListInitExpression result = (ListInitExpression) InvokeAndCheckVisitExpression ("VisitListInitExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.ListInit, result.NodeType);
      Assert.AreSame (newInitializers, result.Initializers);
      Assert.AreSame (expression.NewExpression, result.NewExpression);
    }

    [Test]
    public void VisitElementInit_Unchanged()
    {
      ElementInit elementInit = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1));
      Expect.Call (InvokeVisitExpressionListMethod (elementInit.Arguments)).Return (elementInit.Arguments);

      ElementInit result = (ElementInit) InvokeAndCheckVisitObject ("VisitElementInit", elementInit);
      Assert.AreSame (elementInit, result);
    }

    [Test]
    public void VisitElementInit_Changed ()
    {
      ElementInit elementInit = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1));
      ReadOnlyCollection<Expression> newArguments = new List<Expression>(new Expression[] {Expression.Constant (1)}).AsReadOnly();
      Expect.Call (InvokeVisitExpressionListMethod (elementInit.Arguments)).Return (newArguments);

      ElementInit result = (ElementInit) InvokeAndCheckVisitObject ("VisitElementInit", elementInit);
      Assert.AreNotSame (elementInit, result);
      Assert.AreSame (elementInit.AddMethod, result.AddMethod);
      Assert.AreSame (newArguments, result.Arguments);
    }

    [Test]
    public void VisitMemberBinding_Delegation_MemberAssignment ()
    {
      MemberAssignment memberAssignment = Expression.Bind (typeof (SimpleClass).GetField("Value"), Expression.Constant ("test"));

      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberAssignment)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);
      
      Expect.Call (InvokeVisitMethod ("VisitMemberAssignment", memberAssignment)).Return (memberAssignment);

      MockRepository.ReplayAll();
      object result = InvokeVisitMethod ("VisitMemberBinding", memberAssignment);
      MockRepository.VerifyAll();

      Assert.AreSame (memberAssignment, result);
    }
    
    [Test]
    public void VisitMemberBinding_Delegation_MemberBinding ()
    {
      MemberMemberBinding memberMemberBinding = Expression.MemberBind (typeof (SimpleClass).GetField ("Value"), new List<MemberBinding>());

      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberMemberBinding)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);
      Expect.Call (InvokeVisitMethod ("VisitMemberMemberBinding", memberMemberBinding)).Return (memberMemberBinding);

      MockRepository.ReplayAll ();
      object result = InvokeVisitMethod ("VisitMemberBinding", memberMemberBinding);
      MockRepository.VerifyAll ();

      Assert.AreSame (memberMemberBinding, result);
    }

    [Test]
    public void VisitMemberBinding_Delegation_ListBinding()
    {
      MemberListBinding memberListBinding =
          Expression.ListBind (typeof (SimpleClass).GetField ("Value"), new ElementInit[] {});

    Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberListBinding)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);
    Expect.Call (InvokeVisitMethod ("VisitMemberListBinding", memberListBinding)).Return (memberListBinding);

    MockRepository.ReplayAll ();
    object result = InvokeVisitMethod ("VisitMemberBinding", memberListBinding);
    MockRepository.VerifyAll ();

    Assert.AreSame (memberListBinding, result);
    }

    [Test]
    public void VisitMemberAssignment_Unchanged ()
    {
      MemberAssignment memberAssignment = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("1"));
      Expect.Call (InvokeVisitMethod ("VisitExpression", memberAssignment.Expression)).Return (memberAssignment.Expression);
      MemberAssignment result = (MemberAssignment) InvokeAndCheckVisitObject ("VisitMemberAssignment", memberAssignment);
      Assert.AreSame (memberAssignment, result);
      
    }

    [Test]
    public void VisitMemberAssignment_Changed ()
    {
      MemberAssignment memberAssignment = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant("1"));
      MemberAssignment newMemberAssignment = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("2"));
      
      Expect.Call (InvokeVisitMethod ("VisitExpression", memberAssignment.Expression)).Return (newMemberAssignment.Expression);

      MemberAssignment result = (MemberAssignment) InvokeAndCheckVisitObject ("VisitMemberAssignment",
        memberAssignment);
      Assert.AreNotSame (memberAssignment, result);
    }

    [Test]
    public void VisitMemberMemberBinding_Unchanged ()
    {
      MemberMemberBinding memberMemberBinding = Expression.MemberBind (typeof (SimpleClass).GetField ("Value"), new List<MemberBinding> ());
      Expect.Call (InvokeVisitMethod ("VisitMemberBindingList", memberMemberBinding.Bindings)).Return (memberMemberBinding.Bindings);
      MemberMemberBinding result = (MemberMemberBinding) InvokeAndCheckVisitObject ("VisitMemberMemberBinding", memberMemberBinding);
      Assert.AreSame (memberMemberBinding, result);
    }

    [Test]
    public void VisitMemberMemberBinding_Changed ()
    {
      MemberMemberBinding memberMemberBinding = Expression.MemberBind (typeof (SimpleClass).GetField ("Value"), new List<MemberBinding> ());
      ReadOnlyCollection<MemberBinding> newBindings = new List<MemberBinding>().AsReadOnly();
      Expect.Call (InvokeVisitMethod ("VisitMemberBindingList", memberMemberBinding.Bindings)).Return (newBindings);
      MemberMemberBinding result = (MemberMemberBinding) InvokeAndCheckVisitObject ("VisitMemberMemberBinding", memberMemberBinding);
      Assert.AreNotSame (memberMemberBinding, result);
      Assert.AreSame (newBindings, result.Bindings);
      Assert.AreEqual (memberMemberBinding.BindingType, result.BindingType);
      Assert.AreEqual (memberMemberBinding.Member, result.Member);
    }

    [Test]
    public void VisitMemberListBinding_Unchanged()
    {
      MemberListBinding memberListBinding =
          Expression.ListBind (typeof (SimpleClass).GetField ("Value"), new ElementInit[] { });
      Expect.Call (InvokeVisitMethod ("VisitElementInitList", memberListBinding.Initializers)).Return (memberListBinding.Initializers);
      MemberListBinding result = (MemberListBinding) InvokeAndCheckVisitObject ("VisitMemberListBinding", memberListBinding);
      Assert.AreSame (memberListBinding, result);
    }

    [Test]
    public void VisitMemberListBinding_Changed ()
    {
      MemberListBinding memberListBinding =
          Expression.ListBind (typeof (SimpleClass).GetField ("Value"), new ElementInit[] { });
      ReadOnlyCollection<ElementInit> newInitializers = new List<ElementInit>().AsReadOnly();
      Expect.Call (InvokeVisitMethod ("VisitElementInitList", memberListBinding.Initializers)).Return (newInitializers);
      MemberListBinding result = (MemberListBinding) InvokeAndCheckVisitObject ("VisitMemberListBinding", memberListBinding);
      Assert.AreNotSame (memberListBinding, result);
      Assert.AreSame (newInitializers, result.Initializers);
      Assert.AreEqual (memberListBinding.BindingType, result.BindingType);
      Assert.AreEqual (memberListBinding.Member, result.Member);
    }

    [Test]
    public void VisitExpressionList_Unchanged()
    {
      Expression expr1 = Expression.Constant (1);
      Expression expr2 = Expression.Constant (2);
      ReadOnlyCollection<Expression> expressions = new List<Expression> (new [] {expr1,expr2}).AsReadOnly();
      Expect.Call(InvokeVisitMethod("VisitExpression",expr1)).Return(expr1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expr2)).Return (expr2);
      ReadOnlyCollection<Expression> result = InvokeAndCheckVisitExpressionList (expressions);
      Assert.AreSame (expressions, result);
    }

    [Test]
    public void VisitExpressionList_Changed ()
    {
      Expression expr1 = Expression.Constant (1);
      Expression expr2 = Expression.Constant (2);
      Expression expr3 = Expression.Constant (3);
      Expression newExpression = Expression.Constant (4);
      ReadOnlyCollection<Expression> expressions = new List<Expression> (new [] { expr1, expr2,expr3 }).AsReadOnly ();
      Expect.Call (InvokeVisitMethod ("VisitExpression", expr1)).Return (expr1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expr2)).Return (newExpression);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expr3)).Return (expr3);
      ReadOnlyCollection<Expression> result = InvokeAndCheckVisitExpressionList (expressions);
      Assert.AreNotSame (expressions, result);
      Assert.That (result, Is.EqualTo (new object[] {expr1, newExpression, expr3}));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        ExpectedMessage = "The current list only supports objects of type 'ConstantExpression' as its elements.")]
    public void VisitExpressionList_Changed_InvalidType ()
    {
      ConstantExpression expr1 = Expression.Constant (1);
      ConstantExpression expr2 = Expression.Constant (2);
      ConstantExpression expr3 = Expression.Constant (3);
      ParameterExpression newExpression = Expression.Parameter (typeof (int), "a");

      ReadOnlyCollection<ConstantExpression> expressions = new List<ConstantExpression> (new [] { expr1, expr2, expr3 }).AsReadOnly ();
      Expect.Call (InvokeVisitMethod ("VisitExpression", expr1)).Return (expr1);
      Expect.Call (InvokeVisitMethod<ConstantExpression, ParameterExpression> ("VisitExpression", expr2)).Return (newExpression);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expr3)).Return (expr3);

      try
      {
        InvokeAndCheckVisitExpressionList (expressions);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException;
      }
    }

    [Test]
    public void VisitMemberBindingList_Unchanged ()
    {
      MemberBinding memberBinding1 = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("0"));
      MemberBinding memberBinding2 = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("1"));
      ReadOnlyCollection<MemberBinding> memberBindings = new List<MemberBinding> (new [] { memberBinding1, memberBinding2 }).AsReadOnly ();
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding1)).Return (memberBinding1);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding2)).Return (memberBinding2);
      ReadOnlyCollection<MemberBinding> result = InvokeAndCheckVisitMemberBindingList (memberBindings);
      Assert.AreSame (memberBindings, result);
    }

    [Test]
    public void VisitMemberBindingList_Changed ()
    {
      MemberBinding memberBinding1 = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("0"));
      MemberBinding memberBinding2 = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("1"));
      MemberBinding memberBinding3 = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("2"));
      MemberBinding newMemberBinding = Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("3"));
      ReadOnlyCollection<MemberBinding> memberBindings = new List<MemberBinding> (new [] { memberBinding1, memberBinding2, memberBinding3 }).AsReadOnly ();
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding1)).Return (memberBinding1);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding2)).Return (newMemberBinding);
      Expect.Call (InvokeVisitMethod ("VisitMemberBinding", memberBinding3)).Return (memberBinding3);
      ReadOnlyCollection<MemberBinding> result = InvokeAndCheckVisitMemberBindingList (memberBindings);
      Assert.AreNotSame (memberBindings, result);
      Assert.That (result, Is.EqualTo (new object[] { memberBinding1, newMemberBinding, memberBinding3 }));
    }

    [Test]
    public void VisitElementInitList_Unchanged ()
    {
      ElementInit elementInit1 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (0));
      ElementInit elementInit2 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1));
      ReadOnlyCollection<ElementInit> elementInits = new List<ElementInit> (new [] { elementInit1, elementInit2 }).AsReadOnly ();
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit1)).Return (elementInit1);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit2)).Return (elementInit2);
      ReadOnlyCollection<ElementInit> result = InvokeAndCheckVisitElementInitList (elementInits);
      Assert.AreSame (elementInits, result);
    }

    [Test]
    public void VisitElementInitList_Changed ()
    {
      ElementInit elementInit1 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (0));
      ElementInit elementInit2 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1));
      ElementInit elementInit3 = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (2));
      ElementInit newElementInit = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (3));
      ReadOnlyCollection<ElementInit> elementInits = new List<ElementInit> (new [] { elementInit1, elementInit2, elementInit3 }).AsReadOnly ();
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit1)).Return (elementInit1);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit2)).Return (newElementInit);
      Expect.Call (InvokeVisitMethod ("VisitElementInit", elementInit3)).Return (elementInit3);
      ReadOnlyCollection<ElementInit> result = InvokeAndCheckVisitElementInitList (elementInits);
      Assert.AreNotSame (elementInits, result);
      Assert.That (result, Is.EqualTo (new object[] { elementInit1, newElementInit, elementInit3 }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Expression type -1 is not supported.", MatchType = MessageMatch.Contains)]
    public void VisitUnknownExpression ()
    {
      SpecialExpressionNode expressionNode = new SpecialExpressionNode ((ExpressionType) (-1), typeof (int));
      Expect.Call (InvokeVisitMethod ("VisitUnknownExpression", expressionNode)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);
      MockRepository.ReplayAll();

      try
      {
        InvokeVisitMethod ("VisitUnknownExpression", expressionNode);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException;
      }
    }
  }
}
