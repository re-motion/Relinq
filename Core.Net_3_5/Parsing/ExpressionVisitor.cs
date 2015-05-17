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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing
{
  /// <summary>
  /// Implementation of the .NET 4.0 <b>ExpressionVisitor</b> for .NET 3.5 libraries. This type acts as a base class for the <see cref="RelinqExpressionVisitor"/>.
  /// </summary>
  public abstract class ExpressionVisitor
  {
    public virtual Expression Visit (Expression expression)
    {
      if (expression == null)
        return null;

      var extensionExpression = expression as ExtensionExpression;
      if (extensionExpression != null)
        return extensionExpression.AcceptInternal (this);

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
          return VisitUnary ((UnaryExpression) expression);
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
          return VisitBinary ((BinaryExpression) expression);
        case ExpressionType.Conditional:
          return VisitConditional ((ConditionalExpression) expression);
        case ExpressionType.Constant:
          return VisitConstant ((ConstantExpression) expression);
        case ExpressionType.Invoke:
          return VisitInvocation ((InvocationExpression) expression);
        case ExpressionType.Lambda:
          return VisitLambda ((LambdaExpression) expression);
        case ExpressionType.MemberAccess:
          return VisitMember ((MemberExpression) expression);
        case ExpressionType.Call:
          return VisitMethodCall ((MethodCallExpression) expression);
        case ExpressionType.New:
          return VisitNew ((NewExpression) expression);
        case ExpressionType.NewArrayBounds:
        case ExpressionType.NewArrayInit:
          return VisitNewArray ((NewArrayExpression) expression);
        case ExpressionType.MemberInit:
          return VisitMemberInit ((MemberInitExpression) expression);
        case ExpressionType.ListInit:
          return VisitListInit ((ListInitExpression) expression);
        case ExpressionType.Parameter:
          return VisitParameter ((ParameterExpression) expression);
        case ExpressionType.TypeIs:
          return VisitTypeBinary ((TypeBinaryExpression) expression);

        default:
          return VisitUnknownNonExtension (expression);
      }
    }

    protected internal virtual Expression VisitExtension (ExtensionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression.VisitChildrenInternal (this);
    }

    // There is no longer a case of an unknown expression.
    protected virtual Expression VisitUnknownNonExtension (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var message = string.Format ("Expression type '{0}' is not supported by this {1}.", expression.GetType().Name, GetType ().Name);
      throw new NotSupportedException (message);
    }

    public T VisitAndConvert<T> (T expression, string methodName) where T : Expression
    {
      ArgumentUtility.CheckNotNull ("methodName", methodName);

      if (expression == null)
        return null;

      var newExpression = Visit (expression) as T;

      if (newExpression == null)
      {
        var message = string.Format (
            "When called from '{0}', expressions of type '{1}' can only be replaced with other non-null expressions of type '{2}'.",
            methodName,
            typeof (T).Name,
            typeof (T).Name);

        throw new InvalidOperationException (message);
      }

      return newExpression;
    }

    public ReadOnlyCollection<T> VisitAndConvert<T> (ReadOnlyCollection<T> expressions, string callerName) where T : Expression
    {
      ArgumentUtility.CheckNotNull ("expressions", expressions);
      ArgumentUtility.CheckNotNullOrEmpty ("callerName", callerName);

      return Visit (expressions, expression => VisitAndConvert (expression, callerName));
    }

    public static ReadOnlyCollection<T> Visit<T> (ReadOnlyCollection<T> list, Func<T, T> visitMethod)
        where T : class
    {
      ArgumentUtility.CheckNotNull ("list", list);
      ArgumentUtility.CheckNotNull ("visitMethod", visitMethod);

      List<T> newList = null;

      for (int i = 0; i < list.Count; i++)
      {
        T element = list[i];
        T newElement = visitMethod (element);

        if (element != newElement)
        {
          if (newList == null)
            newList = new List<T> (list);

          newList[i] = newElement;
        }
      }

      if (newList != null)
        return new ReadOnlyCollection<T> (newList);
      else
        return list;
    }

    protected virtual Expression VisitUnary (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newOperand = Visit (expression.Operand);
      if (newOperand != expression.Operand)
      {
        if (expression.NodeType == ExpressionType.UnaryPlus)
          return Expression.UnaryPlus (newOperand, expression.Method);
        else
          return Expression.MakeUnary (expression.NodeType, newOperand, expression.Type, expression.Method);
      }
      else
      {
        return expression;
      }
    }

    protected virtual Expression VisitBinary (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newLeft = Visit (expression.Left);
      var newConversion = VisitAndConvert (expression.Conversion, "VisitBinary");
      Expression newRight = Visit (expression.Right);
      if (newLeft != expression.Left || newRight != expression.Right || newConversion != expression.Conversion)
        return Expression.MakeBinary (expression.NodeType, newLeft, newRight, expression.IsLiftedToNull, expression.Method, newConversion);
      return expression;
    }

    protected virtual Expression VisitTypeBinary (TypeBinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = Visit (expression.Expression);
      if (newExpression == expression.Expression)
        return expression;
      return Expression.TypeIs (newExpression, expression.TypeOperand);
    }

    protected virtual Expression VisitConstant (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression;
    }

    protected virtual Expression VisitConditional (ConditionalExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newTest = Visit (expression.Test);
      Expression newTrue = Visit (expression.IfTrue);
      Expression newFalse = Visit (expression.IfFalse);
      if ((newTest != expression.Test) || (newFalse != expression.IfFalse) || (newTrue != expression.IfTrue))
        return Expression.Condition (newTest, newTrue, newFalse);
      return expression;
    }

    protected virtual Expression VisitParameter (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression;
    }

    protected virtual Expression VisitLambda (LambdaExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newBody = Visit (expression.Body);
      ReadOnlyCollection<ParameterExpression> newParameters = VisitAndConvert (expression.Parameters, "VisitLambda");
      if ((newBody != expression.Body) || (newParameters != expression.Parameters))
        return Expression.Lambda (expression.Type, newBody, newParameters);
      return expression;
    }

    protected virtual Expression VisitMethodCall (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newObject = Visit (expression.Object);
      ReadOnlyCollection<Expression> newArguments = VisitAndConvert (expression.Arguments, "VisitMethodCall");
      if ((newObject != expression.Object) || (newArguments != expression.Arguments))
        return Expression.Call (newObject, expression.Method, newArguments);
      return expression;
    }

    protected virtual Expression VisitInvocation (InvocationExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = Visit (expression.Expression);
      ReadOnlyCollection<Expression> newArguments = VisitAndConvert (expression.Arguments, "VisitInvocation");
      if ((newExpression != expression.Expression) || (newArguments != expression.Arguments))
        return Expression.Invoke (newExpression, newArguments);
      return expression;
    }

    protected virtual Expression VisitMember (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = Visit (expression.Expression);
      if (newExpression != expression.Expression)
        return Expression.MakeMemberAccess (newExpression, expression.Member);
      return expression;
    }

    protected virtual Expression VisitNew (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ReadOnlyCollection<Expression> newArguments = VisitAndConvert (expression.Arguments, "VisitNew");
      if (newArguments != expression.Arguments)
      {
        // This ReSharper warning is wrong - expression.Members can be null

        // ReSharper disable ConditionIsAlwaysTrueOrFalse
        // ReSharper disable HeuristicUnreachableCode
        if (expression.Members == null)
          return Expression.New (expression.Constructor, newArguments);
            // ReSharper restore HeuristicUnreachableCode
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
        else
          return Expression.New (expression.Constructor, newArguments, expression.Members);
      }
      return expression;
    }

    protected virtual Expression VisitNewArray (NewArrayExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ReadOnlyCollection<Expression> newExpressions = VisitAndConvert (expression.Expressions, "VisitNewArray");
      if (newExpressions != expression.Expressions)
      {
        var elementType = expression.Type.GetElementType();
        if (expression.NodeType == ExpressionType.NewArrayInit)
          return Expression.NewArrayInit (elementType, newExpressions);
        else
          return Expression.NewArrayBounds (elementType, newExpressions);
      }
      return expression;
    }

    protected virtual Expression VisitMemberInit (MemberInitExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      var newNewExpression = Visit (expression.NewExpression) as NewExpression;
      if (newNewExpression == null)
      {
        throw new NotSupportedException (
            "MemberInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.");
      }

      ReadOnlyCollection<MemberBinding> newBindings = Visit (expression.Bindings, VisitMemberBinding);
      if (newNewExpression != expression.NewExpression || newBindings != expression.Bindings)
        return Expression.MemberInit (newNewExpression, newBindings);
      return expression;
    }

    protected virtual Expression VisitListInit (ListInitExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      var newNewExpression = Visit (expression.NewExpression) as NewExpression;
      if (newNewExpression == null)
        throw new NotSupportedException ("ListInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.");

      ReadOnlyCollection<ElementInit> newInitializers = Visit (expression.Initializers, VisitElementInit);
      if (newNewExpression != expression.NewExpression || newInitializers != expression.Initializers)
        return Expression.ListInit (newNewExpression, newInitializers);
      return expression;
    }

    protected virtual ElementInit VisitElementInit (ElementInit elementInit)
    {
      ArgumentUtility.CheckNotNull ("elementInit", elementInit);
      ReadOnlyCollection<Expression> newArguments = VisitAndConvert (elementInit.Arguments, "VisitElementInit");
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
          Assertion.DebugAssert (
              memberBinding.BindingType == MemberBindingType.ListBinding, "Invalid member binding type " + memberBinding.GetType().FullName);
          return VisitMemberListBinding ((MemberListBinding) memberBinding);
      }
    }

    protected virtual MemberAssignment VisitMemberAssignment (MemberAssignment memberAssigment)
    {
      ArgumentUtility.CheckNotNull ("memberAssigment", memberAssigment);

      Expression expression = Visit (memberAssigment.Expression);
      if (expression != memberAssigment.Expression)
        return Expression.Bind (memberAssigment.Member, expression);
      return memberAssigment;
    }

    protected virtual MemberMemberBinding VisitMemberMemberBinding (MemberMemberBinding binding)
    {
      ArgumentUtility.CheckNotNull ("binding", binding);

      ReadOnlyCollection<MemberBinding> newBindings = Visit (binding.Bindings, VisitMemberBinding);
      if (newBindings != binding.Bindings)
        return Expression.MemberBind (binding.Member, newBindings);
      return binding;
    }

    protected virtual MemberListBinding VisitMemberListBinding (MemberListBinding listBinding)
    {
      ArgumentUtility.CheckNotNull ("listBinding", listBinding);

      ReadOnlyCollection<ElementInit> newInitializers = Visit (listBinding.Initializers, VisitElementInit);
      if (newInitializers != listBinding.Initializers)
        return Expression.ListBind (listBinding.Member, newInitializers);
      return listBinding;
    }
  }
}