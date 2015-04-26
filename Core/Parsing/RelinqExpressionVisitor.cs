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
  public abstract partial class RelinqExpressionVisitor : ExpressionVisitor
  {
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

    // TODO: ExpressionVisitor.VisitNew does not contain an obvious conversion of the NewExpression's argument types. Is this a problem?
    protected override Expression VisitNew (NewExpression expression)
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
          return Expression.New (expression.Constructor, AdjustArgumentsForNewExpression (newArguments, expression.Members), expression.Members);
      }
      return expression;
    }

    // TODO: Keep on type derived from ExpressionVisitor and refactor SubQueryExpression to use Accept-Visitor.
    // SubQueryExpression accpets visitor and casts to ISubQueryExpressionVisitor, then calls VisitSubQuery.
    // Basic Visitor-implementation returns expression as is.
    // If visitor does not implement interface, do nothing?
    protected internal virtual Expression VisitSubQuery (SubQueryExpression expression)
    {
      return expression;
    }

    // TODO: Keep on type derived from ExpressionVisitor and refactor QuerySourceReferenceExpression to use Accept-Visitor
    // QuerySourceReferenceExpression accpets visitor and casts to IQuerySourceReferenceExpressionVisitor, then calls VisitSubQuery.
    // Basic Visitor-implementation returns expression as is.
    // If visitor does not implement interface, do nothing?
    protected internal virtual Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      return expression;
    }
  }
}