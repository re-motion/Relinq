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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing
{
  /// <summary>
  /// Implements an <see cref="ExpressionTreeVisitor"/> that throws an exception for every expression type that is not explicitly supported.
  /// Inherit from this class to ensure that an exception is thrown when an expression is passed 
  /// </summary>
  public abstract class ThrowingExpressionTreeVisitor : ExpressionTreeVisitor
  {
    protected abstract Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod);

    /// <summary>
    /// Called when an unhandled item is visited. This method provides the item the visitor cannot handle (<paramref name="unhandledItem"/>), 
    /// the <paramref name="visitMethod"/> that is not implemented in the visitor, and a delegate that can be used to invoke the 
    /// <paramref name="baseBehavior"/> of the <see cref="ExpressionTreeVisitor"/> class. The default behavior of this method is to call the
    /// <see cref="CreateUnhandledItemException{T}"/> method, but it can be overridden to do something else.
    /// </summary>
    /// <typeparam name="TItem">The type of the item that could not be handled. Either an <see cref="Expression"/> type, a <see cref="MemberBinding"/> 
    /// type, or <see cref="ElementInit"/>.</typeparam>
    /// <typeparam name="TResult">The result type expected for the visited <paramref name="unhandledItem"/>.</typeparam>
    /// <param name="unhandledItem">The unhandled item.</param>
    /// <param name="visitMethod">The visit method that is not implemented.</param>
    /// <param name="baseBehavior">The behavior exposed by <see cref="ExpressionTreeVisitor"/> for this item type.</param>
    /// <returns>An object to replace <paramref name="unhandledItem"/> in the expression tree. Alternatively, the method can throw any exception.</returns>
    protected virtual TResult VisitUnhandledItem<TItem, TResult> (TItem unhandledItem, string visitMethod, Func<TItem, TResult> baseBehavior)
        where TItem: TResult
    {
      ArgumentUtility.CheckNotNull ("unhandledItem", unhandledItem);
      ArgumentUtility.CheckNotNullOrEmpty ("visitMethod", visitMethod);
      ArgumentUtility.CheckNotNull ("baseBehavior", baseBehavior);

      throw CreateUnhandledItemException (unhandledItem, visitMethod);
    }

    protected internal override Expression VisitExtension (ExtensionExpression expression)
    {
      if (expression.CanReduce)
        return VisitExpression (expression.ReduceAndCheck ());
      else
        return VisitUnhandledItem<ExtensionExpression, Expression> (expression, "VisitExtension", BaseVisitExtension);
    }

    protected Expression BaseVisitExtension (ExtensionExpression expression)
    {
      return base.VisitExtension (expression);
    }

    protected override Expression VisitUnknownNonExtension (Expression expression)
    {
      var expressionAsExtensionExpression = expression as ExtensionExpression;
      if (expressionAsExtensionExpression != null && expressionAsExtensionExpression.CanReduce)
        return VisitExpression (expressionAsExtensionExpression.ReduceAndCheck()); 

      return VisitUnhandledItem<Expression, Expression> (expression, "VisitUnknownNonExtension", BaseVisitUnknownNonExtension);
    }

    protected Expression BaseVisitUnknownNonExtension (Expression expression)
    {
      return base.VisitUnknownNonExtension (expression);
    }

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

    protected override Expression VisitLambda (LambdaExpression expression)
    {
      return VisitUnhandledItem<LambdaExpression, Expression> (expression, "VisitLambda", BaseVisitLambda);
    }

    protected Expression BaseVisitLambda (LambdaExpression expression)
    {
      return base.VisitLambda (expression);
    }

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

    protected override ElementInit VisitElementInit (ElementInit elementInit)
    {
      return VisitUnhandledItem<ElementInit, ElementInit> (elementInit, "VisitElementInit", BaseVisitElementInit);
    }

    protected ElementInit BaseVisitElementInit (ElementInit elementInit)
    {
      return base.VisitElementInit (elementInit);
    }

    protected override MemberBinding VisitMemberAssignment (MemberAssignment memberAssigment)
    {
      return VisitUnhandledItem<MemberAssignment, MemberBinding> (memberAssigment, "VisitMemberAssignment", BaseVisitMemberAssignment);
    }

    protected MemberBinding BaseVisitMemberAssignment (MemberAssignment memberAssigment)
    {
      return base.VisitMemberAssignment (memberAssigment);
    }

    protected override MemberBinding VisitMemberMemberBinding (MemberMemberBinding binding)
    {
      return VisitUnhandledItem<MemberMemberBinding, MemberBinding> (binding, "VisitMemberMemberBinding", BaseVisitMemberMemberBinding);
    }

    protected MemberBinding BaseVisitMemberMemberBinding (MemberMemberBinding binding)
    {
      return base.VisitMemberMemberBinding (binding);
    }

    protected override MemberBinding VisitMemberListBinding (MemberListBinding listBinding)
    {
      return VisitUnhandledItem<MemberListBinding, MemberBinding> (listBinding, "VisitMemberListBinding", BaseVisitMemberListBinding);
    }

    protected MemberBinding BaseVisitMemberListBinding (MemberListBinding listBinding)
    {
      return base.VisitMemberListBinding (listBinding);
    }

    protected override Expression VisitSubQuery (SubQueryExpression expression)
    {
      return VisitUnhandledItem<SubQueryExpression, Expression> (expression, "VisitSubQuery", BaseVisitSubQuery);
    }

    protected Expression BaseVisitSubQuery (SubQueryExpression expression)
    {
      return base.VisitSubQuery (expression);
    }

    protected override Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      return VisitUnhandledItem<QuerySourceReferenceExpression, Expression> (expression, "VisitQuerySourceReference", BaseVisitQuerySourceReference);
    }

    protected Expression BaseVisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      return base.VisitQuerySourceReference (expression);
    }
  }
}
