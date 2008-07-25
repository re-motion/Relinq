/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Remotion.Data.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Visitor
{
  public abstract class ExpressionTreeVisitor
  {
    protected virtual Expression VisitExpression (Expression expression)
    {
      if (expression == null)
        return null;

      switch (expression.NodeType)
      {
        case ExpressionType.ArrayLength:
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
        case ExpressionType.Not:
        case ExpressionType.Quote:
        case ExpressionType.TypeAs:
        case ExpressionType.UnaryPlus:
          return VisitUnaryExpression ((UnaryExpression) expression);
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
        case ExpressionType.Divide:
        case ExpressionType.Modulo:
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
        case ExpressionType.Power:
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
        case ExpressionType.And:
        case ExpressionType.Or:
        case ExpressionType.ExclusiveOr:
        case ExpressionType.LeftShift:
        case ExpressionType.RightShift:
        case ExpressionType.AndAlso:
        case ExpressionType.OrElse:
        case ExpressionType.Equal:
        case ExpressionType.NotEqual:
        case ExpressionType.GreaterThanOrEqual:
        case ExpressionType.GreaterThan:
        case ExpressionType.LessThan:
        case ExpressionType.LessThanOrEqual:
        case ExpressionType.Coalesce:
        case ExpressionType.ArrayIndex:
          return VisitBinaryExpression ((BinaryExpression) expression);
        case ExpressionType.Conditional:
          return VisitConditionalExpression ((ConditionalExpression) expression);
        case ExpressionType.Constant:
          return VisitConstantExpression ((ConstantExpression) expression);
        case ExpressionType.Invoke:
          return VisitInvocationExpression ((InvocationExpression) expression);
        case ExpressionType.Lambda:
          return VisitLambdaExpression ((LambdaExpression) expression);
        case ExpressionType.MemberAccess:
          return VisitMemberExpression ((MemberExpression) expression);
        case ExpressionType.Call:
          return VisitMethodCallExpression ((MethodCallExpression) expression);
        case ExpressionType.New:
          return VisitNewExpression ((NewExpression) expression);
        case ExpressionType.NewArrayBounds:
        case ExpressionType.NewArrayInit:
          return VisitNewArrayExpression ((NewArrayExpression) expression);
        case ExpressionType.MemberInit:
          return VisitMemberInitExpression ((MemberInitExpression) expression);
        case ExpressionType.ListInit:
          return VisitListInitExpression ((ListInitExpression) expression);
        case ExpressionType.Parameter:
          return VisitParameterExpression ((ParameterExpression) expression);
        case ExpressionType.TypeIs:
          return VisitTypeBinaryExpression ((TypeBinaryExpression) expression);

        default:
          if (expression is SubQueryExpression)
            return VisitSubQueryExpression ((SubQueryExpression) expression);
          else
            return VisitUnknownExpression (expression);
      }
    }

    protected virtual Expression VisitUnknownExpression (Expression expression)
    {
      throw new ArgumentException ("Expression type " + expression.NodeType + " is not supported.");
    }

    protected virtual Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newOperand = VisitExpression (expression.Operand);
      if (newOperand != expression.Operand)
      {
        if (expression.NodeType == ExpressionType.UnaryPlus)
          return Expression.UnaryPlus (newOperand, expression.Method);
        else
          return Expression.MakeUnary (expression.NodeType, newOperand, expression.Type, expression.Method);
      }
      else
        return expression;
    }

    protected virtual Expression VisitBinaryExpression (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newLeft = VisitExpression (expression.Left);
      Expression newRight = VisitExpression (expression.Right);
      LambdaExpression newConversion = (LambdaExpression) VisitExpression (expression.Conversion);
      if (newLeft != expression.Left || newRight != expression.Right || newConversion != expression.Conversion)
        return Expression.MakeBinary (expression.NodeType, newLeft, newRight, expression.IsLiftedToNull, expression.Method, newConversion);//, expression.IsLiftedToNull, expression.Method, expression.Conversion);
      return expression;
    }

    protected virtual Expression VisitTypeBinaryExpression (TypeBinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = VisitExpression (expression.Expression);
      if (newExpression != expression.Expression)
        return Expression.TypeIs (newExpression, expression.TypeOperand);
      return expression;
      
    }

    protected virtual Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression;
    }

    protected virtual Expression VisitConditionalExpression (ConditionalExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newTest = VisitExpression (expression.Test);
      Expression newFalse = VisitExpression (expression.IfFalse);
      Expression newTrue = VisitExpression (expression.IfTrue);
      if ((newTest != expression.Test) || (newFalse != expression.IfFalse) || (newTrue != expression.IfTrue))
        return Expression.Condition (newTest, newTrue, newFalse);
      return expression;
    }

    protected virtual Expression VisitParameterExpression (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression;
    }

    protected virtual Expression VisitLambdaExpression (LambdaExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ReadOnlyCollection<ParameterExpression> newParameters = VisitExpressionList (expression.Parameters);
      Expression newBody = VisitExpression (expression.Body);
      if ((newBody != expression.Body) || (newParameters != expression.Parameters))
        return Expression.Lambda (expression.Type, newBody, newParameters);
      return expression;
      
    }

    protected virtual Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newObject = VisitExpression (expression.Object);
      ReadOnlyCollection<Expression> newArguments = VisitExpressionList (expression.Arguments);
      if ((newObject != expression.Object) || (newArguments != expression.Arguments))
        return Expression.Call (newObject, expression.Method, newArguments);
      return expression;
    }

    protected virtual Expression VisitInvocationExpression (InvocationExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = VisitExpression (expression.Expression);
      ReadOnlyCollection<Expression> newArguments = VisitExpressionList (expression.Arguments);
      if ((newExpression != expression.Expression) || (newArguments != expression.Arguments))
        return Expression.Invoke (newExpression, newArguments);
      return expression;
    }
 
    protected virtual Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = VisitExpression (expression.Expression);
      if (newExpression != expression.Expression)
        return Expression.MakeMemberAccess (newExpression, expression.Member);
      return expression;
    }

    protected virtual Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ReadOnlyCollection<Expression> newArguments = VisitExpressionList (expression.Arguments);
      if (newArguments != expression.Arguments)
        return Expression.New (expression.Constructor, newArguments, expression.Members);
      return expression;
    }

    protected virtual Expression VisitNewArrayExpression (NewArrayExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ReadOnlyCollection<Expression> newExpressions = VisitExpressionList (expression.Expressions);
      if (newExpressions != expression.Expressions)
      {
        if (expression.NodeType == ExpressionType.NewArrayInit)
          return Expression.NewArrayInit (expression.Type, newExpressions);
        else
          return Expression.NewArrayBounds (expression.Type, newExpressions);
      }
      return expression;
    }

    protected virtual Expression VisitMemberInitExpression (MemberInitExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      NewExpression newNewExpression = VisitExpression (expression.NewExpression) as NewExpression;
      if (newNewExpression == null)
        throw new NotSupportedException ("MemberInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.");
      ReadOnlyCollection<MemberBinding> newBindings = VisitMemberBindingList (expression.Bindings);
      if (newNewExpression != expression.NewExpression || newBindings != expression.Bindings)
        return Expression.MemberInit (newNewExpression, newBindings);
      return expression;
    }

    protected virtual Expression VisitListInitExpression (ListInitExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      NewExpression newNewExpression = VisitExpression (expression.NewExpression) as NewExpression;
      if (newNewExpression == null)
        throw new NotSupportedException ("ListInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.");
      ReadOnlyCollection<ElementInit> newInitializers = VisitElementInitList (expression.Initializers);
      if (newNewExpression != expression.NewExpression || newInitializers != expression.Initializers)
        return Expression.ListInit (newNewExpression, newInitializers);
      return expression;
    }

    protected virtual ElementInit VisitElementInit (ElementInit elementInit)
    {
      ArgumentUtility.CheckNotNull ("elementInit", elementInit);
      ReadOnlyCollection<Expression> newArguments = VisitExpressionList (elementInit.Arguments);
      if (newArguments != elementInit.Arguments)
        return Expression.ElementInit (elementInit.AddMethod, newArguments);
      return elementInit;
    }

    protected virtual MemberBinding VisitMemberBinding (MemberBinding memberBinding)
    {
      ArgumentUtility.CheckNotNull ("memberBinding", memberBinding);
      switch (memberBinding.BindingType)
      {
        case MemberBindingType.Assignment:
          return VisitMemberAssignment ((MemberAssignment) memberBinding);
        case MemberBindingType.MemberBinding:
          return VisitMemberMemberBinding ((MemberMemberBinding) memberBinding);
        default:
          Assertion.IsTrue (memberBinding.BindingType == MemberBindingType.ListBinding,
              "Invalid member binding type " + memberBinding.GetType ().FullName);
          return VisitMemberListBinding ((MemberListBinding) memberBinding);
      }
    }

    protected virtual MemberAssignment VisitMemberAssignment (MemberAssignment memberAssigment)
    {
      ArgumentUtility.CheckNotNull ("memberAssigment", memberAssigment);

      Expression expression = VisitExpression (memberAssigment.Expression);
      if (expression != memberAssigment.Expression)
      {
        return Expression.Bind (memberAssigment.Member, expression);
      }
      return memberAssigment;
    }

    protected virtual MemberMemberBinding VisitMemberMemberBinding (MemberMemberBinding binding)
    {
      ArgumentUtility.CheckNotNull ("binding", binding);

      ReadOnlyCollection<MemberBinding> newBindings = VisitMemberBindingList (binding.Bindings);
      if (newBindings != binding.Bindings)
      {
        return Expression.MemberBind (binding.Member, newBindings);
      }
      return binding;
      
    }

    protected virtual MemberListBinding VisitMemberListBinding (MemberListBinding listBinding)
    {
      ArgumentUtility.CheckNotNull ("listBinding", listBinding);
      ReadOnlyCollection<ElementInit> newInitializers = VisitElementInitList (listBinding.Initializers);

      if (newInitializers != listBinding.Initializers)
      {
        return Expression.ListBind (listBinding.Member, newInitializers);
      }
      return listBinding;
    }

    protected virtual ReadOnlyCollection<T> VisitExpressionList<T> (ReadOnlyCollection<T> expressions) where T : Expression
    {
      return VisitList (expressions, VisitExpression);
    }

    protected virtual ReadOnlyCollection<MemberBinding> VisitMemberBindingList (ReadOnlyCollection<MemberBinding> expressions)
    {
      return VisitList (expressions, VisitMemberBinding);
    }

    protected virtual ReadOnlyCollection<ElementInit> VisitElementInitList (ReadOnlyCollection<ElementInit> expressions)
    {
      return VisitList (expressions, VisitElementInit);
    }

    protected virtual Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      return expression;
    }

    private ReadOnlyCollection<T> VisitList<T> (ReadOnlyCollection<T> list, Func<T, object> visitMethod)
    where T : class
    {
      List<T> newList = null;

      for (int i = 0; i < list.Count; i++)
      {
        T element = list[i];
        T newElement = visitMethod (element) as T;
        if (newElement == null)
          throw new NotSupportedException ("The current list only supports objects of type '" + typeof (T).Name + "' as its elements.");

        if (element != newElement)
        {
          if (newList == null)
            newList = new List<T> (list);

          newList[i] = newElement;
        }
      }

      if (newList != null)
        return newList.AsReadOnly ();
      else
        return list;
    }
  }
}
