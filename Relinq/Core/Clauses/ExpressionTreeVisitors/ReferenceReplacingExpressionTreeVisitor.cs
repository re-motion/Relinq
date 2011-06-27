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
using System;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.ExpressionTreeVisitors
{
  /// <summary>
  /// Takes an expression and replaces all <see cref="QuerySourceReferenceExpression"/> instances, as defined by a given <see cref="QuerySourceMapping"/>.
  /// This is used whenever references to query sources should be replaced by a transformation.
  /// </summary>
  public class ReferenceReplacingExpressionTreeVisitor : ExpressionTreeVisitor
  {
    /// <summary>
    /// Takes an expression and replaces all <see cref="QuerySourceReferenceExpression"/> instances, as defined by a given 
    /// <paramref name="querySourceMapping"/>.
    /// </summary>
    /// <param name="expression">The expression to be scanned for references.</param>
    /// <param name="querySourceMapping">The clause mapping to be used for replacing <see cref="QuerySourceReferenceExpression"/> instances.</param>
    /// <param name="throwOnUnmappedReferences">If <see langword="true"/>, the visitor will throw an exception when 
    /// <see cref="QuerySourceReferenceExpression"/> not mapped in the <paramref name="querySourceMapping"/> is encountered. If <see langword="false"/>,
    /// the visitor will ignore such expressions.</param>
    /// <returns>An expression with its <see cref="QuerySourceReferenceExpression"/> instances replaced as defined by the 
    /// <paramref name="querySourceMapping"/>.</returns>
    public static Expression ReplaceClauseReferences (Expression expression, QuerySourceMapping querySourceMapping, bool throwOnUnmappedReferences)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("querySourceMapping", querySourceMapping);

      return new ReferenceReplacingExpressionTreeVisitor (querySourceMapping, throwOnUnmappedReferences).VisitExpression (expression);
    }

    private readonly QuerySourceMapping _querySourceMapping;
    private readonly bool _throwOnUnmappedReferences;

    protected ReferenceReplacingExpressionTreeVisitor (QuerySourceMapping querySourceMapping, bool throwOnUnmappedReferences)
    {
      ArgumentUtility.CheckNotNull ("querySourceMapping", querySourceMapping);
      _querySourceMapping = querySourceMapping;
      _throwOnUnmappedReferences = throwOnUnmappedReferences;
    }

    protected QuerySourceMapping QuerySourceMapping
    {
      get { return _querySourceMapping; }
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (QuerySourceMapping.ContainsMapping (expression.ReferencedQuerySource))
      {
        return QuerySourceMapping.GetExpression (expression.ReferencedQuerySource);
      }
      else if (_throwOnUnmappedReferences)
      {
        var message = "Cannot replace reference to clause '" + expression.ReferencedQuerySource.ItemName + "', there is no mapped expression.";
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

      expression.QueryModel.TransformExpressions (ex => ReplaceClauseReferences (ex, _querySourceMapping, _throwOnUnmappedReferences));
      return expression;
    }

    protected internal override Expression VisitUnknownNonExtensionExpression (Expression expression)
    {
      //ignore
      return expression;
    }

  }

}
