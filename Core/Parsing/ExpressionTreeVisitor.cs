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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing
{
  /// <summary>
  /// Provides a base class that can be used for visiting and optionally transforming each node of an <see cref="Expression"/> tree in a 
  /// strongly typed fashion.
  /// This is the base class of many transformation classes.
  /// </summary>
  // TODO: Find name for Relinq-ExpressionVisitor, derived from ExpressionVisitor. 
  // Possibly only needed to provide default implementation for SubQueryExpression and QuerySourceReferenceExpression.
  // May this could be the ThrowingExpressionVisitor and other visitor-implementations that should not be throwing, e.g. ReferenceReplacingETV, 
  // simply implement the interface if they need to handle the custom expressions?
  public abstract class ExpressionTreeVisitor
  {
    /// <summary>
    /// Determines whether the given <see cref="Expression"/> is one of the expressions defined by <see cref="ExpressionType"/> for which
    /// <see cref="ExpressionTreeVisitor"/> has a Visit method. <see cref="VisitExpression"/> handles those by calling the respective Visit method.
    /// </summary>
    /// <param name="expression">The expression to check. Must not be <see langword="null" />.</param>
    /// <returns>
    /// 	<see langword="true"/> if <paramref name="expression"/> is one of the expressions defined by <see cref="ExpressionType"/> and 
    ///   <see cref="ExpressionTreeVisitor"/> has a Visit method for it; otherwise, <see langword="false"/>.
    /// </returns>
    // Note: Do not use Enum.IsDefined here - this method must only return true if we have a dedicated Visit method. (Which may not be the case for
    // future extensions of ExpressionType.)
    //TODO: What should be the new semantic here? 
    // Continue to only support dedicated list?
    // Move this check to EvaluatableTreeFindingExpressionTreeVsitor because that's the only place it is needed?
    // No longer relevant since .NET 4.0?
    public static bool IsSupportedStandardExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

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
        case ExpressionType.Conditional:
        case ExpressionType.Constant:
        case ExpressionType.Invoke:
        case ExpressionType.Lambda:
        case ExpressionType.MemberAccess:
        case ExpressionType.Call:
        case ExpressionType.New:
        case ExpressionType.NewArrayBounds:
        case ExpressionType.NewArrayInit:
        case ExpressionType.MemberInit:
        case ExpressionType.ListInit:
        case ExpressionType.Parameter:
        case ExpressionType.TypeIs:
          return true;
      }
      return false;
    }

    /// <summary>
    /// Determines whether the given <see cref="Expression"/> is one of the base expressions defined by re-linq. 
    /// <see cref="VisitExpression"/> handles those by calling the respective Visit method.
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <returns>
    /// 	<see langword="true"/> if <paramref name="expression"/> is a re-linq base expression (<see cref="SubQueryExpression"/>, 
    ///   <see cref="QuerySourceReferenceExpression"/>) for which <see cref="ExpressionTreeVisitor"/> has dedicated Visit methods;
    ///   otherwise, <see langword="false"/>.
    /// </returns>
    // Should no longer be needed
    public static bool IsRelinqExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      switch (expression.NodeType)
      {
        case SubQueryExpression.ExpressionType:
        case QuerySourceReferenceExpression.ExpressionType:
          return true;

        default:
          return false;
      }
    }

    /// <summary>
    /// Determines whether the given <see cref="Expression"/> is an <see cref="ExtensionExpression"/>. <see cref="VisitExpression"/> handles such
    /// expressions by calling <see cref="ExtensionExpression.Accept"/>.
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <returns>
    /// 	<see langword="true"/> if <paramref name="expression"/> is an <see cref="ExtensionExpression"/>; otherwise, <see langword="false"/>.
    /// </returns>
    // Should no longer be needed
    public static bool IsExtensionExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return expression is ExtensionExpression;
    }

    /// <summary>
    /// Determines whether the given <see cref="Expression"/> is an unknown expression not derived from <see cref="ExtensionExpression"/>. 
    /// <see cref="VisitExpression"/> cannot handle such expressions at all and will call <see cref="VisitUnknownNonExtension"/> for them.
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <returns>
    /// 	<see langword="true"/> if <paramref name="expression"/> is an unknown expression not derived from <see cref="ExtensionExpression"/>; 
    ///   otherwise, <see langword="false"/>.
    /// </returns>
    // Should no longer be needed
    public static bool IsUnknownNonExtensionExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return !IsSupportedStandardExpression (expression) && !IsExtensionExpression (expression) && !IsRelinqExpression (expression);
    }

    /// <summary>
    /// Adjusts the arguments for a <see cref="NewExpression"/> so that they match the given members.
    /// </summary>
    /// <param name="arguments">The arguments to adjust.</param>
    /// <param name="members">The members defining the required argument types.</param>
    /// <returns>
    /// A sequence of expressions that are equivalent to <paramref name="arguments"/>, but converted to the associated member's
    /// result type if needed.
    /// </returns>
    // TODO: Used by ExpressionTreeVisitor.VisitNew() and MemberAddingNewExpressionTransformerBase.Transform()
    // VisitNew() does not perform a conversion in .NET 4.0. Was introduced in re-linq with RM-3712, commit 359923a931b6859cd75108041f4e320cd37aa596
    // Is the conversion needed in Transfom()? Conversion has been introduced in RM-3631, commit 545bd33dc63095de9720ce3ee10cabd0c9448994, 
    // so probably will just need to be moved back there.
    public static IEnumerable<Expression> AdjustArgumentsForNewExpression (IList<Expression> arguments, IList<MemberInfo> members)
    {
      ArgumentUtility.CheckNotNull ("arguments", arguments);
      ArgumentUtility.CheckNotNull ("members", members);

      Assertion.IsTrue (arguments.Count == members.Count);

      for (int i = 0; i < arguments.Count; ++i)
      {
        var memberReturnType = ReflectionUtility.GetMemberReturnType (members[i]);
        if (arguments[i].Type == memberReturnType)
          yield return arguments[i];
        else
          yield return Expression.Convert (arguments[i], memberReturnType);
      }
    }

    // Remove, is provided by ExpressionVisitor
    // TODO: What to do with all the unsupported Expressions, e.g. Loop, Goto, Try, etc?
    public virtual Expression VisitExpression (Expression expression)
    {
      if (expression == null)
        return null;

      var extensionExpression = expression as ExtensionExpression;
      if (extensionExpression != null)
        return extensionExpression.Accept (this);

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

        case SubQueryExpression.ExpressionType:
          return VisitSubQueryExpression ((SubQueryExpression) expression);
        case QuerySourceReferenceExpression.ExpressionType:
          return VisitQuerySourceReferenceExpression ((QuerySourceReferenceExpression) expression);

        default:
          return VisitUnknownNonExtension (expression);
      }
    }

    // Remove, is provided by ExpressionVisitor, just not virtual
    public virtual T VisitAndConvert<T> (T expression, string methodName) where T : Expression
    {
      ArgumentUtility.CheckNotNull ("methodName", methodName);

      if (expression == null)
        return null;

      var newExpression = VisitExpression (expression) as T;

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
    public virtual ReadOnlyCollection<T> VisitAndConvert<T> (ReadOnlyCollection<T> expressions, string callerName) where T : Expression
    {
      ArgumentUtility.CheckNotNull ("expressions", expressions);
      ArgumentUtility.CheckNotNullOrEmpty ("callerName", callerName);

      return VisitList (expressions, expression => VisitAndConvert (expression, callerName));
    }

    // Replace with ExpressionVisitor.Visit<T> (ReadOnlyCollection<T>, Func<T,T>)
    public ReadOnlyCollection<T> VisitList<T> (ReadOnlyCollection<T> list, Func<T, T> visitMethod)
        where T : class
    {
      ArgumentUtility.CheckNotNull ("list", list);
      ArgumentUtility.CheckNotNull ("visitMethod", visitMethod);

      List<T> newList = null;

      for (int i = 0; i < list.Count; i++)
      {
        T element = list[i];
        T newElement = visitMethod (element);
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
        return new ReadOnlyCollection<T> (newList);
      else
        return list;
    }

    // Identical implemention by ExpressionVisitor, but there is no "ExtensionExpression". Instead, "Expression" is already extensible.
    // The existing implementation requires ExtensionExpression to provide an implementation for VistChildren, otherwise a NotSupportedException is thrown.
    // In .NET 4.0, the Expression must be marked as CanReduce, otherwise an ArgumentException is thrown.
    // TODO: ExtensionExpression can be moved to .NET 3.5 implementation
    protected internal virtual Expression VisitExtension (ExtensionExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression.VisitChildren (this);
    }

    // There is no longer a case of an unknown expression.
    protected virtual Expression VisitUnknownNonExtension (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var message = string.Format ("Expression type '{0}' is not supported by this {1}.", expression.GetType().Name, GetType ().Name);
      throw new NotSupportedException (message);
    }

    // Identical implemention by ExpressionVisitor, special handling for UnaryPlus is now handled by Expression.MakeUnary
    protected virtual Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newOperand = VisitExpression (expression.Operand);
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
    protected virtual Expression VisitBinaryExpression (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newLeft = VisitExpression (expression.Left);
      var newConversion = (LambdaExpression) VisitExpression (expression.Conversion);
      Expression newRight = VisitExpression (expression.Right);
      if (newLeft != expression.Left || newRight != expression.Right || newConversion != expression.Conversion)
        return Expression.MakeBinary (expression.NodeType, newLeft, newRight, expression.IsLiftedToNull, expression.Method, newConversion);
      return expression;
    }

    // ExpressionVisitor.VisitTypeBinary differentiates between TypeIs and TypeEqual by supplying a different ExpressionType
    protected virtual Expression VisitTypeBinaryExpression (TypeBinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = VisitExpression (expression.Expression);
      if (newExpression == expression.Expression)
        return expression;
#if !NET_3_5
      if (expression.NodeType == ExpressionType.TypeEqual)
        return Expression.TypeEqual(newExpression, expression.TypeOperand);
#endif
      return Expression.TypeIs (newExpression, expression.TypeOperand);
    }

    // Identical implemention by ExpressionVisitor
    protected virtual Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression;
    }

    // ExpressionVisitor.VisitConditional visits Test, IfTrue, IfFalse, current implementation visits Test, IfFalse, IfTrue
    protected virtual Expression VisitConditionalExpression (ConditionalExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newTest = VisitExpression (expression.Test);
      Expression newTrue = VisitExpression (expression.IfTrue);
      Expression newFalse = VisitExpression (expression.IfFalse);
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
    protected virtual Expression VisitParameterExpression (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression;
    }

    // TODO: ExpressionVisitor.VisitLambda has no overload accepting LamdaExpression, only Expression<T>, derived from LambdaExpression. Is this a problem?
    // ExpressionVisitor.VisitBinary visits Body, Parameters, current implementation visits Parameters, Body
    protected virtual Expression VisitLambdaExpression (LambdaExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newBody = VisitExpression (expression.Body);
      ReadOnlyCollection<ParameterExpression> newParameters = VisitAndConvert (expression.Parameters, "VisitLambdaExpression");
      if ((newBody != expression.Body) || (newParameters != expression.Parameters))
        return Expression.Lambda (expression.Type, newBody, newParameters);
      return expression;
    }

    // ExpressionVisitor.VisitBinary creates specialized MethodCallExpressions. Should not make a difference.
    protected virtual Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newObject = VisitExpression (expression.Object);
      ReadOnlyCollection<Expression> newArguments = VisitAndConvert (expression.Arguments, "VisitMethodCallExpression");
      if ((newObject != expression.Object) || (newArguments != expression.Arguments))
        return Expression.Call (newObject, expression.Method, newArguments);
      return expression;
    }

    // Identical implemention by ExpressionVisitor
    protected virtual Expression VisitInvocationExpression (InvocationExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = VisitExpression (expression.Expression);
      ReadOnlyCollection<Expression> newArguments = VisitAndConvert (expression.Arguments, "VisitInvocationExpression");
      if ((newExpression != expression.Expression) || (newArguments != expression.Arguments))
        return Expression.Invoke (newExpression, newArguments);
      return expression;
    }

    // Identical implemention by ExpressionVisitor
    protected virtual Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = VisitExpression (expression.Expression);
      if (newExpression != expression.Expression)
        return Expression.MakeMemberAccess (newExpression, expression.Member);
      return expression;
    }

    // TODO: ExpressionVisitor.VisitNew does not contain an obvious conversion of the NewExpression's argument types. Is this a problem?
    protected virtual Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ReadOnlyCollection<Expression> newArguments = VisitAndConvert (expression.Arguments, "VisitNewExpression");
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
          return Expression.New (expression.Constructor, AdjustArgumentsForNewExpression (newArguments, expression.Members), expression.Members);
      }
      return expression;
    }

    // Identical implemention by ExpressionVisitor
    protected virtual Expression VisitNewArrayExpression (NewArrayExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ReadOnlyCollection<Expression> newExpressions = VisitAndConvert (expression.Expressions, "VisitNewArrayExpression");
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
    protected virtual Expression VisitMemberInitExpression (MemberInitExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      var newNewExpression = VisitExpression (expression.NewExpression) as NewExpression;
      if (newNewExpression == null)
      {
        throw new NotSupportedException (
            "MemberInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.");
      }

      ReadOnlyCollection<MemberBinding> newBindings = VisitMemberBindingList (expression.Bindings);
      if (newNewExpression != expression.NewExpression || newBindings != expression.Bindings)
        return Expression.MemberInit (newNewExpression, newBindings);
      return expression;
    }

    // Identical implemention by ExpressionVisitor
    protected virtual Expression VisitListInitExpression (ListInitExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      var newNewExpression = VisitExpression (expression.NewExpression) as NewExpression;
      if (newNewExpression == null)
        throw new NotSupportedException ("ListInitExpressions only support non-null instances of type 'NewExpression' as their NewExpression member.");
      ReadOnlyCollection<ElementInit> newInitializers = VisitElementInitList (expression.Initializers);
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
    protected virtual MemberBinding VisitMemberAssignment (MemberAssignment memberAssigment)
    {
      ArgumentUtility.CheckNotNull ("memberAssigment", memberAssigment);

      Expression expression = VisitExpression (memberAssigment.Expression);
      if (expression != memberAssigment.Expression)
        return Expression.Bind (memberAssigment.Member, expression);
      return memberAssigment;
    }

    // Identical implemention by ExpressionVisitor
    protected virtual MemberBinding VisitMemberMemberBinding (MemberMemberBinding binding)
    {
      ArgumentUtility.CheckNotNull ("binding", binding);

      ReadOnlyCollection<MemberBinding> newBindings = VisitMemberBindingList (binding.Bindings);
      if (newBindings != binding.Bindings)
        return Expression.MemberBind (binding.Member, newBindings);
      return binding;
    }

    // Identical implemention by ExpressionVisitor
    protected virtual MemberBinding VisitMemberListBinding (MemberListBinding listBinding)
    {
      ArgumentUtility.CheckNotNull ("listBinding", listBinding);
      ReadOnlyCollection<ElementInit> newInitializers = VisitElementInitList (listBinding.Initializers);

      if (newInitializers != listBinding.Initializers)
        return Expression.ListBind (listBinding.Member, newInitializers);
      return listBinding;
    }

    // Identical implemention by ExpressionVisitor, just no extension point
    protected virtual ReadOnlyCollection<MemberBinding> VisitMemberBindingList (ReadOnlyCollection<MemberBinding> expressions)
    {
      return VisitList (expressions, VisitMemberBinding);
    }

    // Identical implemention by ExpressionVisitor, just no extension point
    protected virtual ReadOnlyCollection<ElementInit> VisitElementInitList (ReadOnlyCollection<ElementInit> expressions)
    {
      return VisitList (expressions, VisitElementInit);
    }

    // TODO: Keep on type derived from ExpressionVisitor and refactor SubQueryExpression to use Accept-Visitor.
    // SubQueryExpression accpets visitor and casts to ISubQueryExpressionVisitor, then calls VisitSubQuery.
    // Basic Visitor-implementation returns expression as is.
    // If visitor does not implement interface, do nothing?
    protected virtual Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      return expression;
    }

    // TODO: Keep on type derived from ExpressionVisitor and refactor QuerySourceReferenceExpression to use Accept-Visitor
    // QuerySourceReferenceExpression accpets visitor and casts to IQuerySourceReferenceExpressionVisitor, then calls VisitSubQuery.
    // Basic Visitor-implementation returns expression as is.
    // If visitor does not implement interface, do nothing?
    protected virtual Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      return expression;
    }

    [Obsolete ("This method has been split. Use VisitExtensionExpression or VisitUnknownNonExtensionExpression instead. 1.13.75")]
    protected internal virtual Expression VisitUnknownExpression (Expression expression)
    {
      throw new NotImplementedException ();
    }
  }
}