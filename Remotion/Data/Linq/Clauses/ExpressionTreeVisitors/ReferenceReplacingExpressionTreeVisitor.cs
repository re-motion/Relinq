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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ExpressionTreeVisitors
{
  /// <summary>
  /// Takes an expression and replaces all <see cref="QuerySourceReferenceExpression"/> instances, as defined by a given <see cref="ClauseMapping"/>.
  /// This is used whenever references to query sources should be replaced by a transformation.
  /// </summary>
  public class ReferenceReplacingExpressionTreeVisitor : ExpressionTreeVisitor
  {
    /// <summary>
    /// Takes an expression and replaces all <see cref="QuerySourceReferenceExpression"/> instances, as defined by a given 
    /// <paramref name="clauseMapping"/>.
    /// </summary>
    /// <param name="expression">The expression to be scanned for references.</param>
    /// <param name="clauseMapping">The clause mapping to be used for replacing <see cref="QuerySourceReferenceExpression"/> instances.</param>
    /// <param name="throwOnUnmappedReferences">If <see langword="true"/>, the visitor will throw an exception when 
    /// <see cref="QuerySourceReferenceExpression"/> not mapped in the <paramref name="clauseMapping"/> is encountered. If <see langword="false"/>,
    /// the visitor will ignore such expressions.</param>
    /// <returns>An expression with its <see cref="QuerySourceReferenceExpression"/> instances replaced as defined by the 
    /// <paramref name="clauseMapping"/>.</returns>
    public static Expression ReplaceClauseReferences (Expression expression, ClauseMapping clauseMapping, bool throwOnUnmappedReferences)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("clauseMapping", clauseMapping);

      return new ReferenceReplacingExpressionTreeVisitor (clauseMapping, throwOnUnmappedReferences).VisitExpression (expression);
    }

    private readonly ClauseMapping _clauseMapping;
    private readonly bool _throwOnUnmappedReferences;

    protected ReferenceReplacingExpressionTreeVisitor (ClauseMapping clauseMapping, bool throwOnUnmappedReferences)
    {
      ArgumentUtility.CheckNotNull ("clauseMapping", clauseMapping);
      _clauseMapping = clauseMapping;
      _throwOnUnmappedReferences = throwOnUnmappedReferences;
    }

    protected ClauseMapping ClauseMapping
    {
      get { return _clauseMapping; }
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (ClauseMapping.ContainsMapping (expression.ReferencedClause))
      {
        return ClauseMapping.GetExpression (expression.ReferencedClause);
      }
      else if (_throwOnUnmappedReferences)
      {
        var message = "Cannot replace reference to clause '" + expression.ReferencedClause.ItemName + "', there is no mapped expression.";
        throw new InvalidOperationException (message);
      }
      else
      {
        return expression;
      }
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      expression.QueryModel.TransformExpressions (ex => ReplaceClauseReferences (ex, _clauseMapping, _throwOnUnmappedReferences));
      return expression;
    }

    protected override Expression VisitUnknownExpression (Expression expression)
    {
      //ignore
      return expression;
    }

  }

}