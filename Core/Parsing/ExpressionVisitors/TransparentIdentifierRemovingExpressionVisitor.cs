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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Utilities;
using MemberBinding = Remotion.Linq.Parsing.ExpressionVisitors.MemberBindings.MemberBinding;

namespace Remotion.Linq.Parsing.ExpressionVisitors
{
  /// <summary>
  /// Replaces expression patterns of the form <c>new T { x = 1, y = 2 }.x</c> (<see cref="MemberInitExpression"/>) or 
  /// <c>new T ( x = 1, y = 2 ).x</c> (<see cref="NewExpression"/>) to <c>1</c> (or <c>2</c> if <c>y</c> is accessed instead of <c>x</c>).
  /// Expressions are also replaced within subqueries; the <see cref="QueryModel"/> is changed by the replacement operations, it is not copied. 
  /// </summary>
  public sealed class TransparentIdentifierRemovingExpressionVisitor : RelinqExpressionVisitor
  {
    public static Expression ReplaceTransparentIdentifiers (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      Expression expressionBeforeRemove;
      Expression expressionAfterRemove = expression;

      // Run again and again until no replacements have been made.
      do
      {
        expressionBeforeRemove = expressionAfterRemove;
        expressionAfterRemove = new TransparentIdentifierRemovingExpressionVisitor().Visit (expressionAfterRemove);
      } while (expressionAfterRemove != expressionBeforeRemove);

      return expressionAfterRemove;
    }

    private TransparentIdentifierRemovingExpressionVisitor ()
    {
    }

    protected override Expression VisitMember (MemberExpression memberExpression)
    {
      ArgumentUtility.CheckNotNull ("memberExpression", memberExpression);

      var memberBindings = GetMemberBindingsCreatedByExpression (memberExpression.Expression);
      if (memberBindings == null)
        return base.VisitMember (memberExpression);

      var matchingAssignment = memberBindings
          .Where (binding => binding.MatchesReadAccess (memberExpression.Member))
          .LastOrDefault();

      if (matchingAssignment == null)
        return base.VisitMember (memberExpression);
      else
        return matchingAssignment.AssociatedExpression;
    }

    protected internal override Expression VisitSubQuery (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      expression.QueryModel.TransformExpressions (ReplaceTransparentIdentifiers);
      return expression; // Note that we modifiy the (mutable) QueryModel, we return an unchanged expression
    }

#if NET_3_5
    protected override Expression VisitRelinqUnknownNonExtension (Expression expression)
    {
      //ignore
      return expression;
    }
#endif

    private IEnumerable<MemberBinding> GetMemberBindingsCreatedByExpression (Expression expression)
    {
      var memberInitExpression = expression as MemberInitExpression;
      if (memberInitExpression != null)
      {
        return memberInitExpression.Bindings
            .Where (binding => binding is MemberAssignment)
            .Select (assignment => MemberBinding.Bind (assignment.Member, ((MemberAssignment) assignment).Expression));
      }
      else
      {
        var newExpression = expression as NewExpression;
        if (newExpression != null && newExpression.Members != null)
          return GetMemberBindingsForNewExpression (newExpression);
        else
          return null;
      }
    }

    private IEnumerable<MemberBinding> GetMemberBindingsForNewExpression (NewExpression newExpression)
    {
      for (int i = 0; i < newExpression.Members.Count; ++i)
        yield return MemberBinding.Bind (newExpression.Members[i], newExpression.Arguments[i]);
    }
  }
}
