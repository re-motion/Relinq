using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using System.Linq.Expressions;
using Rhino.Mocks;
using System.Reflection;
using Rhino.Mocks.Interfaces;
using Rubicon.Data.Linq.Visitor;

namespace Rubicon.Data.Linq.UnitTests.VisitorTest.ExpressionTreeVisitorTest
{
  [TestFixture]
  public class VisitSpecificExpressionsTest
  {
    private MockRepository _mockRepository;
    private ExpressionTreeVisitor _visitorMock;

    [SetUp]
    public void Setup()
    {
      _mockRepository = new MockRepository();
      _visitorMock = _mockRepository.CreateMock<ExpressionTreeVisitor>();
    }

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
    public void VisitBinaryExpression_Unchanged ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Add);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Left)).Return (expression.Left);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Right)).Return (expression.Right);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisitExpression ("VisitBinaryExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitBinaryExpression_LeftChanged ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Subtract);
      Expression newOperand = Expression.Constant (1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Left)).Return (newOperand);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Right)).Return (expression.Right);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisitExpression ("VisitBinaryExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Subtract, result.NodeType);
      Assert.AreSame (newOperand, result.Left);
      Assert.AreSame (expression.Right, result.Right);
    }

    [Test]
    public void VisitBinaryExpression_RightChanged ()
    {
      BinaryExpression expression = (BinaryExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Subtract);
      Expression newOperand = Expression.Constant (1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Left)).Return (expression.Left);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Right)).Return (newOperand);

      BinaryExpression result = (BinaryExpression) InvokeAndCheckVisitExpression ("VisitBinaryExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.Subtract, result.NodeType);
      Assert.AreSame (expression.Left, result.Left);
      Assert.AreSame (newOperand, result.Right);
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
      Expect.Call (InvokeVisitListMethod (expression.Parameters)).Return (expression.Parameters);
      LambdaExpression result = (LambdaExpression) InvokeAndCheckVisitExpression ("VisitLambdaExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitLambdaExpression_ChangedBody ()
    {
      LambdaExpression expression = (LambdaExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Lambda);
      Expression newBody = Expression.Constant (1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Body)).Return (newBody);
      Expect.Call (InvokeVisitListMethod (expression.Parameters)).Return (expression.Parameters);
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
      Expect.Call (InvokeVisitListMethod (expression.Parameters)).Return (newParameters);
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
      Expect.Call (InvokeVisitListMethod (expression.Arguments)).Return (expression.Arguments);
      MethodCallExpression result = (MethodCallExpression) InvokeAndCheckVisitExpression ("VisitMethodCallExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitMethodCallExpression_ChangedObject ()
    {
      MethodCallExpression expression = (MethodCallExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Call);
      Expression newObject = Expression.Constant (1);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Object)).Return (newObject);
      Expect.Call (InvokeVisitListMethod (expression.Arguments)).Return (expression.Arguments);
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
      Expect.Call (InvokeVisitListMethod (expression.Arguments)).Return (newParameters);
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
      Expect.Call (InvokeVisitListMethod (expression.Arguments)).Return (expression.Arguments);
      InvocationExpression result = (InvocationExpression) InvokeAndCheckVisitExpression ("VisitInvocationExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitInvocationExpression_ChangedObject ()
    {
      InvocationExpression expression = (InvocationExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Invoke);
      Expression newExpression = Expression.Lambda (Expression.Constant (1));
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.Expression)).Return (newExpression);
      Expect.Call (InvokeVisitListMethod (expression.Arguments)).Return (expression.Arguments);
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
      Expect.Call (InvokeVisitListMethod (expression.Arguments)).Return (newParameters);
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
      Expect.Call (InvokeVisitListMethod (expression.Arguments)).Return (expression.Arguments);
      NewExpression result = (NewExpression) InvokeAndCheckVisitExpression ("VisitNewExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitNewExpression_ChangedArguments ()
    {
      NewExpression expression = (NewExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.New);
      ReadOnlyCollection<Expression> newArguments = new List<Expression> ().AsReadOnly ();
      Expect.Call (InvokeVisitListMethod (expression.Arguments)).Return (newArguments);
      NewExpression result = (NewExpression) InvokeAndCheckVisitExpression ("VisitNewExpression", expression);
      Assert.AreNotSame (expression, result);
      Assert.AreEqual (ExpressionType.New, result.NodeType);
      Assert.AreSame (newArguments, result.Arguments);
    }

    [Test]
    public void VisitNewArrayExpression_Unchanged ()
    {
      NewArrayExpression expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayInit);
      Expect.Call (InvokeVisitListMethod (expression.Expressions)).Return (expression.Expressions);
      NewArrayExpression result = (NewArrayExpression) InvokeAndCheckVisitExpression ("VisitNewArrayExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    public void VisitNewArrayInitExpression_Changed ()
    {
      NewArrayExpression expression = (NewArrayExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.NewArrayInit);
      ReadOnlyCollection<Expression> newExpressions = new List<Expression> ().AsReadOnly ();
      Expect.Call (InvokeVisitListMethod (expression.Expressions)).Return (newExpressions);
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
      Expect.Call (InvokeVisitListMethod (expression.Expressions)).Return (newExpressions);
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
      Expect.Call (InvokeVisitMemberBindingListMethod (expression.Bindings)).Return (expression.Bindings);
      MemberInitExpression result = (MemberInitExpression) InvokeAndCheckVisitExpression ("VisitMemberInitExpression", expression);
      Assert.AreSame (expression, result);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException),
        "MemberInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.")]
    public void VisitMemberInitExpression_InvalidNewExpression ()
    {
      MemberInitExpression expression = (MemberInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.MemberInit);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.NewExpression)).Return (Expression.Constant (0));
      Expect.Call (InvokeVisitMemberBindingListMethod (expression.Bindings)).Return (expression.Bindings);
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
      Expression newNewExpression = Expression.New (typeof (List<int>));
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.NewExpression)).Return (newNewExpression);
      Expect.Call (InvokeVisitMemberBindingListMethod (expression.Bindings)).Return (expression.Bindings);
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
      Expect.Call (InvokeVisitMemberBindingListMethod (expression.Bindings)).Return (newBindings);
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
      Expect.Call (InvokeVisitElementInitListMethod (expression.Initializers)).Return (expression.Initializers);
      ListInitExpression result = (ListInitExpression) InvokeAndCheckVisitExpression ("VisitListInitExpression", expression);
      Assert.AreSame (expression, result);
    }
    
    [Test]
    [ExpectedException (typeof (NotSupportedException),
        "ListInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.")]
    public void VisitListInitExpression_InvalidNewExpression ()
    {
      ListInitExpression expression = (ListInitExpression) ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.ListInit);
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.NewExpression)).Return (Expression.Constant (0));
      Expect.Call (InvokeVisitElementInitListMethod (expression.Initializers)).Return (expression.Initializers);
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
      Expression newNewExpression = Expression.New (typeof (List<int>));
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.NewExpression)).Return (newNewExpression);
      Expect.Call (InvokeVisitElementInitListMethod (expression.Initializers)).Return (expression.Initializers);
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
          new List<ElementInit> (new ElementInit[] {Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1))}).AsReadOnly ();
      Expect.Call (InvokeVisitMethod ("VisitExpression", expression.NewExpression)).Return (expression.NewExpression);
      Expect.Call (InvokeVisitElementInitListMethod (expression.Initializers)).Return (newInitializers);
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
      Expect.Call (InvokeVisitListMethod (elementInit.Arguments)).Return (elementInit.Arguments);

      ElementInit result = (ElementInit) InvokeAndCheckVisitObject ("VisitElementInit", elementInit);
      Assert.AreSame (elementInit, result);
    }

    [Test]
    public void VisitElementInit_Changed ()
    {
      ElementInit elementInit = Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1));
      ReadOnlyCollection<Expression> newArguments = new List<Expression>(new Expression[] {Expression.Constant (1)}).AsReadOnly();
      Expect.Call (InvokeVisitListMethod (elementInit.Arguments)).Return (newArguments);

      ElementInit result = (ElementInit) InvokeAndCheckVisitObject ("VisitElementInit", elementInit);
      Assert.AreNotSame (elementInit, result);
      Assert.AreSame (elementInit.AddMethod, result.AddMethod);
      Assert.AreSame (newArguments, result.Arguments);
    }

    [Test]
    [Ignore]
    public void VisitMemberBinding_Unchanged ()
    {
      
    }

    private Expression InvokeAndCheckVisitExpression (string methodName, Expression expression)
    {
      return (Expression) InvokeAndCheckVisitObject (methodName, expression);
    }

    private object InvokeAndCheckVisitObject (string methodName, object argument)
    {
      Expect.Call (InvokeVisitMethod (methodName, argument)).CallOriginalMethod (OriginalCallOptions.CreateExpectation);

      _mockRepository.ReplayAll ();

      object result = InvokeVisitMethod (methodName, argument);
      _mockRepository.VerifyAll ();

      return result;
    }

    private object InvokeVisitMethod (string methodName, object expression)
    {
      return _visitorMock.GetType().GetMethod (methodName, BindingFlags.NonPublic | BindingFlags.Instance).Invoke (_visitorMock, new object[] {expression});
    }

    private ReadOnlyCollection<MemberBinding> InvokeVisitMemberBindingListMethod (ReadOnlyCollection<MemberBinding> expressions)
    {
      return (ReadOnlyCollection<MemberBinding>) _visitorMock.GetType ().GetMethod ("VisitMemberBindingList", BindingFlags.NonPublic | BindingFlags.Instance)
        .Invoke (_visitorMock, new object[] { expressions });
    }

    private ReadOnlyCollection<ElementInit> InvokeVisitElementInitListMethod (ReadOnlyCollection<ElementInit> expressions)
    {
      return (ReadOnlyCollection<ElementInit>) _visitorMock.GetType ().GetMethod ("VisitElementInitList", BindingFlags.NonPublic | BindingFlags.Instance)
        .Invoke (_visitorMock, new object[] { expressions });
    }

    private ReadOnlyCollection<T> InvokeVisitListMethod<T> (ReadOnlyCollection<T> expressions) where T : Expression
    {
      return (ReadOnlyCollection<T>) _visitorMock.GetType ().GetMethod ("VisitExpressionList", BindingFlags.NonPublic | BindingFlags.Instance)
        .MakeGenericMethod (typeof (T))
        .Invoke (_visitorMock, new object[] { expressions });
    }
  }
}