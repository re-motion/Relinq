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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing
{
  /// <summary>
  /// Implements an <see cref="RelinqExpressionVisitor"/> that throws an exception for every expression type that is not explicitly supported.
  /// Inherit from this class to ensure that an exception is thrown when an expression is passed 
  /// </summary>
  public abstract partial class ThrowingExpressionVisitor : RelinqExpressionVisitor
  {
#if !NET_3_5
    private static readonly Assembly s_systemLinqAssembly = typeof (Expression).GetTypeInfo().Assembly;
#endif

    protected abstract Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod);

#if !NET_3_5
    public override Expression Visit (Expression expression)
    {
      if (expression == null)
        return base.Visit ((Expression) null);

      var isCustomExpression = !ReferenceEquals (s_systemLinqAssembly, expression.GetType().GetTypeInfo().Assembly);
      if (isCustomExpression)
        return base.Visit (expression);

      var isWellKnownStandardExpression = IsWellKnownStandardExpression (expression);
      if (isWellKnownStandardExpression)
        return base.Visit (expression);

      return VisitUnknownStandardExpression (expression, "Visit" + expression.NodeType, base.Visit);
    }

    protected virtual Expression VisitUnknownStandardExpression (Expression expression, string visitMethod, Func<Expression, Expression> baseBehavior)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      throw CreateUnhandledItemException (expression, visitMethod);
    }

    private bool IsWellKnownStandardExpression (Expression expression)
    {
      if (expression is UnaryExpression)
        return true;
      if (expression is BinaryExpression)
        return true;
      if (expression is TypeBinaryExpression)
        return true;
      if (expression is ConstantExpression)
        return true;
      if (expression is ConditionalExpression)
        return true;
      if (expression is ParameterExpression)
        return true;
      if (expression is LambdaExpression)
        return true;
      if (expression is MethodCallExpression)
        return true;
      if (expression is InvocationExpression)
        return true;
      if (expression is MemberExpression)
        return true;
      if (expression is NewExpression)
        return true;
      if (expression is NewArrayExpression)
        return true;
      if (expression is MemberInitExpression)
        return true;
      if (expression is ListInitExpression)
        return true;
      if (expression is BlockExpression)
        return true;
      if (expression is DebugInfoExpression)
        return true;
      if (expression is DefaultExpression)
        return true;
      if (expression is GotoExpression)
        return true;
      if (expression is IndexExpression)
        return true;
      if (expression is LabelExpression)
        return true;
      if (expression is LoopExpression)
        return true;
      if (expression is RuntimeVariablesExpression)
        return true;
      if (expression is SwitchExpression)
        return true;
      if (expression is TryExpression)
        return true;
      return false;
    }
#endif

    /// <summary>
    /// Called when an unhandled item is visited. This method provides the item the visitor cannot handle (<paramref name="unhandledItem"/>), 
    /// the <paramref name="visitMethod"/> that is not implemented in the visitor, and a delegate that can be used to invoke the 
    /// <paramref name="baseBehavior"/> of the <see cref="RelinqExpressionVisitor"/> class. The default behavior of this method is to call the
    /// <see cref="CreateUnhandledItemException{T}"/> method, but it can be overridden to do something else.
    /// </summary>
    /// <typeparam name="TItem">The type of the item that could not be handled. Either an <see cref="Expression"/> type, a <see cref="MemberBinding"/> 
    /// type, or <see cref="ElementInit"/>.</typeparam>
    /// <typeparam name="TResult">The result type expected for the visited <paramref name="unhandledItem"/>.</typeparam>
    /// <param name="unhandledItem">The unhandled item.</param>
    /// <param name="visitMethod">The visit method that is not implemented.</param>
    /// <param name="baseBehavior">The behavior exposed by <see cref="RelinqExpressionVisitor"/> for this item type.</param>
    /// <returns>An object to replace <paramref name="unhandledItem"/> in the expression tree. Alternatively, the method can throw any exception.</returns>
    protected virtual TResult VisitUnhandledItem<TItem, TResult> (TItem unhandledItem, string visitMethod, Func<TItem, TResult> baseBehavior)
        where TItem: TResult
    {
      ArgumentUtility.CheckNotNull ("unhandledItem", unhandledItem);
      ArgumentUtility.CheckNotNullOrEmpty ("visitMethod", visitMethod);
      ArgumentUtility.CheckNotNull ("baseBehavior", baseBehavior);

      throw CreateUnhandledItemException (unhandledItem, visitMethod);
    }

#if !NET_3_5
    protected override Expression VisitExtension (Expression expression)
    {
      if (expression.CanReduce)
        return Visit (expression.ReduceAndCheck());
      else
        return VisitUnhandledItem<Expression, Expression> (expression, "VisitExtension", BaseVisitExtension);
    }

    protected Expression BaseVisitExtension (Expression expression)
    {
      return base.VisitExtension (expression);
    }
#else
    protected internal override Expression VisitExtension (ExtensionExpression expression)
    {
      if (expression.CanReduce)
        return Visit (expression.ReduceAndCheck());
      else
        return VisitUnhandledItem<ExtensionExpression, Expression> (expression, "VisitExtension", BaseVisitExtension);
    }

    protected Expression BaseVisitExtension (ExtensionExpression expression)
    {
      return base.VisitExtension (expression);
    }
#endif

    protected override Expression VisitUnary (UnaryExpression expression)
    {
      return VisitUnhandledItem<UnaryExpression, Expression> (expression, "VisitUnary", BaseVisitUnary);
    }

    protected Expression BaseVisitUnary (UnaryExpression expression)
    {
      return base.VisitUnary (expression);
    }

    protected override Expression VisitBinary (BinaryExpression expression)
    {
      return VisitUnhandledItem<BinaryExpression, Expression> (expression, "VisitBinary", BaseVisitBinary);
    }

    protected Expression BaseVisitBinary (BinaryExpression expression)
    {
      return base.VisitBinary (expression);
    }

    protected override Expression VisitTypeBinary (TypeBinaryExpression expression)
    {
      return VisitUnhandledItem<TypeBinaryExpression, Expression> (expression, "VisitTypeBinary", BaseVisitTypeBinary);
    }

    protected Expression BaseVisitTypeBinary (TypeBinaryExpression expression)
    {
      return base.VisitTypeBinary (expression);
    }

    protected override Expression VisitConstant (ConstantExpression expression)
    {
      return VisitUnhandledItem<ConstantExpression, Expression> (expression, "VisitConstant", BaseVisitConstant);
    }

    protected Expression BaseVisitConstant (ConstantExpression expression)
    {
      return base.VisitConstant (expression);
    }

    protected override Expression VisitConditional (ConditionalExpression expression)
    {
      return VisitUnhandledItem<ConditionalExpression, Expression> (expression, "VisitConditional", BaseVisitConditional);
    }

    protected Expression BaseVisitConditional (ConditionalExpression arg)
    {
      return base.VisitConditional (arg);
    }

    protected override Expression VisitParameter (ParameterExpression expression)
    {
      return VisitUnhandledItem<ParameterExpression, Expression> (expression, "VisitParameter", BaseVisitParameter);
    }

    protected Expression BaseVisitParameter (ParameterExpression expression)
    {
      return base.VisitParameter (expression);
    }

#if !NET_3_5
    protected override Expression VisitLambda<T> (Expression<T> expression)
    {
      return VisitUnhandledItem<Expression<T>, Expression> (expression, "VisitLambda", BaseVisitLambda);
    }

    protected Expression BaseVisitLambda<T> (Expression<T> expression)
    {
      return base.VisitLambda (expression);
    }
#else
    protected override Expression VisitLambda (LambdaExpression expression)
    {
      return VisitUnhandledItem<LambdaExpression, Expression> (expression, "VisitLambda", BaseVisitLambda);
    }

    protected Expression BaseVisitLambda (LambdaExpression expression)
    {
      return base.VisitLambda (expression);
    }
#endif

    protected override Expression VisitMethodCall (MethodCallExpression expression)
    {
      return VisitUnhandledItem<MethodCallExpression, Expression> (expression, "VisitMethodCall", BaseVisitMethodCall);
    }

    protected Expression BaseVisitMethodCall (MethodCallExpression expression)
    {
      return base.VisitMethodCall (expression);
    }

    protected override Expression VisitInvocation (InvocationExpression expression)
    {
      return VisitUnhandledItem<InvocationExpression, Expression> (expression, "VisitInvocation", BaseVisitInvocation);
    }

    protected Expression BaseVisitInvocation (InvocationExpression expression)
    {
      return base.VisitInvocation (expression);
    }

    protected override Expression VisitMember (MemberExpression expression)
    {
      return VisitUnhandledItem<MemberExpression, Expression> (expression, "VisitMember", BaseVisitMember);
    }

    protected Expression BaseVisitMember (MemberExpression expression)
    {
      return base.VisitMember (expression);
    }

    protected override Expression VisitNew (NewExpression expression)
    {
      return VisitUnhandledItem<NewExpression, Expression> (expression, "VisitNew", BaseVisitNew);
    }

    protected Expression BaseVisitNew (NewExpression expression)
    {
      return base.VisitNew (expression);
    }

    protected override Expression VisitNewArray (NewArrayExpression expression)
    {
      return VisitUnhandledItem<NewArrayExpression, Expression> (expression, "VisitNewArray", BaseVisitNewArray);
    }

    protected Expression BaseVisitNewArray (NewArrayExpression expression)
    {
      return base.VisitNewArray (expression);
    }

    protected override Expression VisitMemberInit (MemberInitExpression expression)
    {
      return VisitUnhandledItem<MemberInitExpression, Expression> (expression, "VisitMemberInit", BaseVisitMemberInit);
    }

    protected Expression BaseVisitMemberInit (MemberInitExpression expression)
    {
      return base.VisitMemberInit (expression);
    }

    protected override Expression VisitListInit (ListInitExpression expression)
    {
      return VisitUnhandledItem<ListInitExpression, Expression> (expression, "VisitListInit", BaseVisitListInit);
    }

    protected Expression BaseVisitListInit (ListInitExpression expression)
    {
      return base.VisitListInit (expression);
    }

#if !NET_3_5
    protected override Expression VisitBlock (BlockExpression expression)
    {
      return VisitUnhandledItem<BlockExpression, Expression> (expression, "VisitBlock", BaseVisitBlock);
    }

    protected Expression BaseVisitBlock (BlockExpression expression)
    {
      return base.VisitBlock (expression);
    }

    protected override Expression VisitDebugInfo (DebugInfoExpression expression)
    {
      return VisitUnhandledItem<DebugInfoExpression, Expression> (expression, "VisitDebugInfo", BaseVisitDebugInfo);
    }

    protected Expression BaseVisitDebugInfo (DebugInfoExpression expression)
    {
      return base.VisitDebugInfo (expression);
    }

    protected override Expression VisitDefault (DefaultExpression expression)
    {
      return VisitUnhandledItem<DefaultExpression, Expression> (expression, "VisitDefault", BaseVisitDefault);
    }

    protected Expression BaseVisitDefault (DefaultExpression expression)
    {
      return base.VisitDefault (expression);
    }

    protected override Expression VisitGoto (GotoExpression expression)
    {
      return VisitUnhandledItem<GotoExpression, Expression> (expression, "VisitGoto", BaseVisitGoto);
    }

    protected Expression BaseVisitGoto (GotoExpression expression)
    {
      return base.VisitGoto (expression);
    }

    protected override Expression VisitIndex (IndexExpression expression)
    {
      return VisitUnhandledItem<IndexExpression, Expression> (expression, "VisitIndex", BaseVisitIndex);
    }

    protected Expression BaseVisitIndex (IndexExpression expression)
    {
      return base.VisitIndex (expression);
    }

    protected override Expression VisitLabel (LabelExpression expression)
    {
      return VisitUnhandledItem<LabelExpression, Expression> (expression, "VisitLabel", BaseVisitLabel);
    }

    protected Expression BaseVisitLabel (LabelExpression expression)
    {
      return base.VisitLabel (expression);
    }

    protected override Expression VisitLoop (LoopExpression expression)
    {
      return VisitUnhandledItem<LoopExpression, Expression> (expression, "VisitLoop", BaseVisitLoop);
    }

    protected Expression BaseVisitLoop (LoopExpression expression)
    {
      return base.VisitLoop (expression);
    }

    protected override Expression VisitRuntimeVariables (RuntimeVariablesExpression expression)
    {
      return VisitUnhandledItem<RuntimeVariablesExpression, Expression> (expression, "VisitRuntimeVariables", BaseVisitRuntimeVariables);
    }

    protected Expression BaseVisitRuntimeVariables (RuntimeVariablesExpression expression)
    {
      return base.VisitRuntimeVariables (expression);
    }

    protected override Expression VisitSwitch (SwitchExpression expression)
    {
      return VisitUnhandledItem<SwitchExpression, Expression> (expression, "VisitSwitch", BaseVisitSwitch);
    }

    protected Expression BaseVisitSwitch (SwitchExpression expression)
    {
      return base.VisitSwitch (expression);
    }

    protected override Expression VisitTry (TryExpression expression)
    {
      return VisitUnhandledItem<TryExpression, Expression> (expression, "VisitTry", BaseVisitTry);
    }

    protected Expression BaseVisitTry (TryExpression expression)
    {
      return base.VisitTry (expression);
    }
#endif

#if !NET_3_5
    protected override MemberBinding VisitMemberBinding (MemberBinding expression)
    {
      // Base-implementation will already delegate all variations of MemberBinding to dedicated Vist-methods.
      // Therefor, the VisitMemberBinding-method should not throw an exception on its own.
      return BaseVisitMemberBinding (expression);
    }

    protected MemberBinding BaseVisitMemberBinding (MemberBinding expression)
    {
      return base.VisitMemberBinding (expression);
    }
#endif

    protected override ElementInit VisitElementInit (ElementInit elementInit)
    {
      return VisitUnhandledItem<ElementInit, ElementInit> (elementInit, "VisitElementInit", BaseVisitElementInit);
    }

    protected ElementInit BaseVisitElementInit (ElementInit elementInit)
    {
      return base.VisitElementInit (elementInit);
    }

    protected override MemberAssignment VisitMemberAssignment (MemberAssignment memberAssigment)
    {
      return VisitUnhandledItem<MemberAssignment, MemberAssignment> (memberAssigment, "VisitMemberAssignment", BaseVisitMemberAssignment);
    }

    protected MemberAssignment BaseVisitMemberAssignment (MemberAssignment memberAssigment)
    {
      return base.VisitMemberAssignment (memberAssigment);
    }

    protected override MemberMemberBinding VisitMemberMemberBinding (MemberMemberBinding binding)
    {
      return VisitUnhandledItem<MemberMemberBinding, MemberMemberBinding> (binding, "VisitMemberMemberBinding", BaseVisitMemberMemberBinding);
    }

    protected MemberMemberBinding BaseVisitMemberMemberBinding (MemberMemberBinding binding)
    {
      return base.VisitMemberMemberBinding (binding);
    }

    protected override MemberListBinding VisitMemberListBinding (MemberListBinding listBinding)
    {
      return VisitUnhandledItem<MemberListBinding, MemberListBinding> (listBinding, "VisitMemberListBinding", BaseVisitMemberListBinding);
    }

    protected MemberListBinding BaseVisitMemberListBinding (MemberListBinding listBinding)
    {
      return base.VisitMemberListBinding (listBinding);
    }

#if !NET_3_5
    protected override CatchBlock VisitCatchBlock (CatchBlock expression)
    {
      return VisitUnhandledItem<CatchBlock, CatchBlock> (expression, "VisitCatchBlock", BaseVisitCatchBlock);
    }

    protected CatchBlock BaseVisitCatchBlock (CatchBlock expression)
    {
      return base.VisitCatchBlock (expression);
    }

    protected override LabelTarget VisitLabelTarget (LabelTarget expression)
    {
      return VisitUnhandledItem<LabelTarget, LabelTarget> (expression, "VisitLabelTarget", BaseVisitLabelTarget);
    }

    protected LabelTarget BaseVisitLabelTarget (LabelTarget expression)
    {
      return base.VisitLabelTarget (expression);
    }


    protected override SwitchCase VisitSwitchCase (SwitchCase expression)
    {
      return VisitUnhandledItem<SwitchCase, SwitchCase> (expression, "VisitSwitchCase", BaseVisitSwitchCase);
    }

    protected SwitchCase BaseVisitSwitchCase (SwitchCase expression)
    {
      return base.VisitSwitchCase (expression);
    }
#endif

    protected internal override Expression VisitSubQuery (SubQueryExpression expression)
    {
      return VisitUnhandledItem<SubQueryExpression, Expression> (expression, "VisitSubQuery", BaseVisitSubQuery);
    }

    protected Expression BaseVisitSubQuery (SubQueryExpression expression)
    {
      return base.VisitSubQuery (expression);
    }

    protected internal override Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      return VisitUnhandledItem<QuerySourceReferenceExpression, Expression> (expression, "VisitQuerySourceReference", BaseVisitQuerySourceReference);
    }

    protected Expression BaseVisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      return base.VisitQuerySourceReference (expression);
    }
  }
}
