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
  public abstract class ExpressionVisitor
  {
    // Remove, is provided by ExpressionVisitor
    // TODO: What to do with all the unsupported Expressions, e.g. Loop, Goto, Try, etc?
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

    // Identical implemention by ExpressionVisitor, but there is no "ExtensionExpression". Instead, "Expression" is already extensible.
    // The existing implementation requires ExtensionExpression to provide an implementation for VistChildren, otherwise a NotSupportedException is thrown.
    // In .NET 4.0, the Expression must be marked as CanReduce, otherwise an ArgumentException is thrown.
    // TODO: ExtensionExpression can be moved to .NET 3.5 implementation
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

    // Remove, is provided by ExpressionVisitor, just not virtual
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

    // Remove, is provided by ExpressionVisitor, just not virtual
    public ReadOnlyCollection<T> VisitAndConvert<T> (ReadOnlyCollection<T> expressions, string callerName) where T : Expression
    {
      ArgumentUtility.CheckNotNull ("expressions", expressions);
      ArgumentUtility.CheckNotNullOrEmpty ("callerName", callerName);

      return Visit (expressions, expression => VisitAndConvert (expression, callerName));
    }

    // Replace with ExpressionVisitor.Visit<T> (ReadOnlyCollection<T>, Func<T,T>)
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

    // Identical implemention by ExpressionVisitor, special handling for UnaryPlus is now handled by Expression.MakeUnary
    protected virtual Expression VisitUnary (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newOperand = Visit (expression.Operand);
      if (newOperand != expression.Operand)
      {
#if NET_3_5
        if (expression.NodeType == ExpressionType.UnaryPlus)
          return Expression.UnaryPlus (newOperand, expression.Method);
#endif
        return Expression.MakeUnary (expression.NodeType, newOperand, expression.Type, expression.Method);
      }
      else
        return expression;
    }

    // ExpressionVisitor.VisitBinary visits Left, Conversion, Right, current implementation visits Left, Right, Conversion
    // ExpressionVisitor.VisitBinary creates LogicalBinaryExpression for reference equality check. Should not make a difference.
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

    // ExpressionVisitor.VisitTypeBinary differentiates between TypeIs and TypeEqual by supplying a different ExpressionType
    protected virtual Expression VisitTypeBinary (TypeBinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = Visit (expression.Expression);
      if (newExpression == expression.Expression)
        return expression;
#if !NET_3_5
      if (expression.NodeType == ExpressionType.TypeEqual)
        return Expression.TypeEqual(newExpression, expression.TypeOperand);
#endif
      return Expression.TypeIs (newExpression, expression.TypeOperand);
    }

    // Identical implemention by ExpressionVisitor
    protected virtual Expression VisitConstant (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression;
    }

    // ExpressionVisitor.VisitConditional visits Test, IfTrue, IfFalse, current implementation visits Test, IfFalse, IfTrue
    protected virtual Expression VisitConditional (ConditionalExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newTest = Visit (expression.Test);
      Expression newTrue = Visit (expression.IfTrue);
      Expression newFalse = Visit (expression.IfFalse);
      if ((newTest != expression.Test) || (newFalse != expression.IfFalse) || (newTrue != expression.IfTrue))
      {
#if !NET_3_5
        return Expression.Condition (newTest, newTrue, newFalse, expression.Type);
#else
        return Expression.Condition (newTest, newTrue, newFalse);
#endif
      }
      return expression;
    }

    // Identical implemention by ExpressionVisitor
    protected virtual Expression VisitParameter (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression;
    }

    // TODO: ExpressionVisitor.VisitLambda has no overload accepting LamdaExpression, only Expression<T>, derived from LambdaExpression. Is this a problem?
    // ExpressionVisitor.VisitBinary visits Body, Parameters, current implementation visits Parameters, Body
    protected virtual Expression VisitLambda (LambdaExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newBody = Visit (expression.Body);
      ReadOnlyCollection<ParameterExpression> newParameters = VisitAndConvert (expression.Parameters, "VisitLambda");
      if ((newBody != expression.Body) || (newParameters != expression.Parameters))
        return Expression.Lambda (expression.Type, newBody, newParameters);
      return expression;
    }

    // ExpressionVisitor.VisitBinary creates specialized MethodCallExpressions. Should not make a difference.
    protected virtual Expression VisitMethodCall (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newObject = Visit (expression.Object);
      ReadOnlyCollection<Expression> newArguments = VisitAndConvert (expression.Arguments, "VisitMethodCall");
      if ((newObject != expression.Object) || (newArguments != expression.Arguments))
        return Expression.Call (newObject, expression.Method, newArguments);
      return expression;
    }

    // Identical implemention by ExpressionVisitor
    protected virtual Expression VisitInvocation (InvocationExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = Visit (expression.Expression);
      ReadOnlyCollection<Expression> newArguments = VisitAndConvert (expression.Arguments, "VisitInvocation");
      if ((newExpression != expression.Expression) || (newArguments != expression.Arguments))
        return Expression.Invoke (newExpression, newArguments);
      return expression;
    }

    // Identical implemention by ExpressionVisitor
    protected virtual Expression VisitMember (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = Visit (expression.Expression);
      if (newExpression != expression.Expression)
        return Expression.MakeMemberAccess (newExpression, expression.Member);
      return expression;
    }

    // TODO: ExpressionVisitor.VisitNew does not contain an obvious conversion of the NewExpression's argument types. Is this a problem?
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

    // Identical implemention by ExpressionVisitor
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

    // Identical implemention by ExpressionVisitor
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

    // Identical implemention by ExpressionVisitor
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

    // Identical implemention by ExpressionVisitor
    protected virtual ElementInit VisitElementInit (ElementInit elementInit)
    {
      ArgumentUtility.CheckNotNull ("elementInit", elementInit);
      ReadOnlyCollection<Expression> newArguments = VisitAndConvert (elementInit.Arguments, "VisitElementInit");
      if (newArguments != elementInit.Arguments)
        return Expression.ElementInit (elementInit.AddMethod, newArguments);
      return elementInit;
    }

    // Identical implemention by ExpressionVisitor
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

    // Identical implemention by ExpressionVisitor
    protected virtual MemberAssignment VisitMemberAssignment (MemberAssignment memberAssigment)
    {
      ArgumentUtility.CheckNotNull ("memberAssigment", memberAssigment);

      Expression expression = Visit (memberAssigment.Expression);
      if (expression != memberAssigment.Expression)
        return Expression.Bind (memberAssigment.Member, expression);
      return memberAssigment;
    }

    // Identical implemention by ExpressionVisitor
    protected virtual MemberMemberBinding VisitMemberMemberBinding (MemberMemberBinding binding)
    {
      ArgumentUtility.CheckNotNull ("binding", binding);

      ReadOnlyCollection<MemberBinding> newBindings = Visit (binding.Bindings, VisitMemberBinding);
      if (newBindings != binding.Bindings)
        return Expression.MemberBind (binding.Member, newBindings);
      return binding;
    }

    // Identical implemention by ExpressionVisitor
    protected virtual MemberListBinding VisitMemberListBinding (MemberListBinding listBinding)
    {
      ArgumentUtility.CheckNotNull ("listBinding", listBinding);

      ReadOnlyCollection<ElementInit> newInitializers = Visit (listBinding.Initializers, VisitElementInit);
      if (newInitializers != listBinding.Initializers)
        return Expression.ListBind (listBinding.Member, newInitializers);
      return listBinding;
    }

    [Obsolete ("This method has been split. Use VisitExtensionExpression or VisitUnknownNonExtensionExpression instead. 1.13.75")]
    protected internal virtual Expression VisitUnknownExpression (Expression expression)
    {
      throw new NotImplementedException ();
    }
  }
}