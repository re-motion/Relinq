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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing
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
        where TItem : TResult
    {
      ArgumentUtility.CheckNotNull ("unhandledItem", unhandledItem);
      ArgumentUtility.CheckNotNullOrEmpty ("visitMethod", visitMethod);
      ArgumentUtility.CheckNotNull ("baseBehavior", baseBehavior);

      throw CreateUnhandledItemException (unhandledItem, visitMethod);
    }

    protected override Expression VisitUnknownExpression (Expression expression)
    {
      return VisitUnhandledItem<Expression, Expression> (expression, "VisitUnknownExpression", BaseVisitUnknownExpression);
    }

    protected Expression BaseVisitUnknownExpression (Expression expression)
    {
      return base.VisitUnknownExpression (expression);
    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      return VisitUnhandledItem<UnaryExpression, Expression> (expression, "VisitUnaryExpression", BaseVisitUnaryExpression);
    }

    protected Expression BaseVisitUnaryExpression (UnaryExpression expression)
    {
      return base.VisitUnaryExpression (expression);
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      return VisitUnhandledItem<BinaryExpression, Expression> (expression, "VisitBinaryExpression", BaseVisitBinaryExpression);
    }

    protected Expression BaseVisitBinaryExpression (BinaryExpression expression)
    {
      return base.VisitBinaryExpression (expression);
    }

    protected override Expression VisitTypeBinaryExpression (TypeBinaryExpression expression)
    {
      return VisitUnhandledItem<TypeBinaryExpression, Expression> (expression, "VisitTypeBinaryExpression", BaseVisitTypeBinaryExpression);
    }

    protected Expression BaseVisitTypeBinaryExpression (TypeBinaryExpression expression)
    {
      return base.VisitTypeBinaryExpression (expression);
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      return VisitUnhandledItem<ConstantExpression, Expression> (expression, "VisitConstantExpression", BaseVisitConstantExpression);
    }

    protected Expression BaseVisitConstantExpression (ConstantExpression expression)
    {
      return base.VisitConstantExpression (expression);
    }

    protected override Expression VisitConditionalExpression (ConditionalExpression expression)
    {
      return VisitUnhandledItem<ConditionalExpression, Expression> (expression, "VisitConditionalExpression", BaseVisitConditionalExpression);
    }

    protected Expression BaseVisitConditionalExpression (ConditionalExpression arg)
    {
      return base.VisitConditionalExpression (arg);
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      return VisitUnhandledItem<ParameterExpression, Expression> (expression, "VisitParameterExpression", BaseVisitParameterExpression);
    }

    protected Expression BaseVisitParameterExpression (ParameterExpression expression)
    {
      return base.VisitParameterExpression (expression);
    }

    protected override Expression VisitLambdaExpression (LambdaExpression expression)
    {
      return VisitUnhandledItem<LambdaExpression, Expression> (expression, "VisitLambdaExpression", BaseVisitLambdaExpression);
    }

    protected Expression BaseVisitLambdaExpression (LambdaExpression expression)
    {
      return base.VisitLambdaExpression (expression);
    }

    protected override Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      return VisitUnhandledItem<MethodCallExpression, Expression> (expression, "VisitMethodCallExpression", BaseVisitMethodCallExpression);
    }

    protected Expression BaseVisitMethodCallExpression (MethodCallExpression expression)
    {
      return base.VisitMethodCallExpression (expression);
    }

    protected override Expression VisitInvocationExpression (InvocationExpression expression)
    {
      return VisitUnhandledItem<InvocationExpression, Expression> (expression, "VisitInvocationExpression", BaseVisitInvocationExpression);
    }

    protected Expression BaseVisitInvocationExpression (InvocationExpression expression)
    {
      return base.VisitInvocationExpression (expression);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      return VisitUnhandledItem<MemberExpression, Expression> (expression, "VisitMemberExpression", BaseVisitMemberExpression);
    }

    protected Expression BaseVisitMemberExpression (MemberExpression expression)
    {
      return base.VisitMemberExpression (expression);
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      return VisitUnhandledItem<NewExpression, Expression> (expression, "VisitNewExpression", BaseVisitNewExpression);
    }

    protected Expression BaseVisitNewExpression (NewExpression expression)
    {
      return base.VisitNewExpression (expression);
    }

    protected override Expression VisitNewArrayExpression (NewArrayExpression expression)
    {
      return VisitUnhandledItem<NewArrayExpression, Expression> (expression, "VisitNewArrayExpression", BaseVisitNewArrayExpression);
    }

    protected Expression BaseVisitNewArrayExpression (NewArrayExpression expression)
    {
      return base.VisitNewArrayExpression (expression);
    }

    protected override Expression VisitMemberInitExpression (MemberInitExpression expression)
    {
      return VisitUnhandledItem<MemberInitExpression, Expression> (expression, "VisitMemberInitExpression", BaseVisitMemberInitExpression);
    }

    protected Expression BaseVisitMemberInitExpression (MemberInitExpression expression)
    {
      return base.VisitMemberInitExpression (expression);
    }

    protected override Expression VisitListInitExpression (ListInitExpression expression)
    {
      return VisitUnhandledItem<ListInitExpression, Expression> (expression, "VisitListInitExpression", BaseVisitListInitExpression);
    }

    protected Expression BaseVisitListInitExpression (ListInitExpression expression)
    {
      return base.VisitListInitExpression (expression);
    }

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

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      return VisitUnhandledItem<SubQueryExpression, Expression> (expression, "VisitSubQueryExpression", BaseVisitSubQueryExpression);
    }

    protected Expression BaseVisitSubQueryExpression (SubQueryExpression expression)
    {
      return base.VisitSubQueryExpression (expression);
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      return VisitUnhandledItem<QuerySourceReferenceExpression, Expression> (expression, "VisitQuerySourceReferenceExpression", BaseVisitQuerySourceReferenceExpression);
    }

    protected Expression BaseVisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      return base.VisitQuerySourceReferenceExpression (expression);
    }
  }
}