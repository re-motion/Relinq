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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  /// <summary>
  /// removes transparent identifier from a expression representing a field access
  /// </summary>
  public class QueryModelFieldResolverVisitor : ExpressionTreeVisitor
  {
    private readonly QueryModel _queryModel;

    private IResolveableClause _clause;
    private bool _hackNeeded; // TODO 1096: Remove.

    private QueryModelFieldResolverVisitor (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      _queryModel = queryModel;
      _clause = null;
      _hackNeeded = false; // TODO 1096: Remove.
    }

    public static QueryModelFieldResolverVisitorResult ParseAndReduce (QueryModel queryModel, Expression expression)
    {
      var visitor = new QueryModelFieldResolverVisitor (queryModel);

      Expression reducedExpression = visitor.VisitExpression (expression);
      if (visitor._clause != null || visitor._hackNeeded)
        return new QueryModelFieldResolverVisitorResult (reducedExpression, visitor._clause, visitor._hackNeeded);
      else
        return null;
    }

    // TODO 1218: Store clause of QuerySourceReferenceExpression if such an expression is found
    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _clause = _queryModel.GetResolveableClause (expression.Name, expression.Type);

      if (_clause == null && expression.Name.StartsWith ("<generated>")) // TODO 1096: Remove.
        _hackNeeded = true; // TODO 1096: Remove.

      if (_clause != null)
        return base.VisitParameterExpression (expression);
      else
        return null; //this is a transparent identifier
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _clause = expression.ReferencedClause;
      return base.VisitQuerySourceReferenceExpression (expression);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression newExpression = VisitExpression (expression.Expression);
      if (newExpression == null)
      {
        //found a transparent identifier, ignore it and continue with the next expression
        ParameterExpression newParameterExpression = Expression.Parameter (expression.Type, expression.Member.Name);
        return VisitExpression (newParameterExpression);
      }
      else if (newExpression != expression.Expression)
        return Expression.MakeMemberAccess (newExpression, expression.Member);
      else
        return expression;
    }
  }
}
