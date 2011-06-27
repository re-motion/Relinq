// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Utilities;
using MemberBinding = Remotion.Linq.Parsing.ExpressionTreeVisitors.MemberBindings.MemberBinding;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors
{
  /// <summary>
  /// Replaces expression patterns of the form <c>new T { x = 1, y = 2 }.x</c> (<see cref="MemberInitExpression"/>) or 
  /// <c>new T ( x = 1, y = 2 ).x</c> (<see cref="NewExpression"/>) to <c>1</c> (or <c>2</c> if <c>y</c> is accessed instead of <c>x</c>).
  /// Expressions are also replaced within subqueries; the <see cref="QueryModel"/> is changed by the replacement operations, it is not copied. 
  /// </summary>
  public class TransparentIdentifierRemovingExpressionTreeVisitor : ExpressionTreeVisitor
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
        expressionAfterRemove = new TransparentIdentifierRemovingExpressionTreeVisitor().VisitExpression (expressionAfterRemove);
      } while (expressionAfterRemove != expressionBeforeRemove);

      return expressionAfterRemove;
    }

    private TransparentIdentifierRemovingExpressionTreeVisitor ()
    {
    }

    protected override Expression VisitMemberExpression (MemberExpression memberExpression)
    {
      var memberBindings = GetMemberBindingsCreatedByExpression (memberExpression.Expression);
      if (memberBindings == null)
        return base.VisitMemberExpression (memberExpression);

      var matchingAssignment = memberBindings
          .Where (binding => binding.MatchesReadAccess (memberExpression.Member))
          .LastOrDefault();

      if (matchingAssignment == null)
        return base.VisitMemberExpression (memberExpression);
      else
        return matchingAssignment.AssociatedExpression;
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      expression.QueryModel.TransformExpressions (ReplaceTransparentIdentifiers);
      return expression; // Note that we modifiy the (mutable) QueryModel, we return an unchanged expression
    }

    protected internal override Expression VisitUnknownNonExtensionExpression (Expression expression)
    {
      //ignore
      return expression;
    }

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
