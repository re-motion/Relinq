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
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.ExpressionTreeVisitors
{
  /// <summary>
  /// Visits an <see cref="Expression"/> tree, replacing all <see cref="QuerySourceReferenceExpression"/> instances with references to cloned clauses,
  /// as defined by a <see cref="QuerySourceMapping"/>. In addition, all <see cref="QueryModel"/> instances in 
  /// <see cref="SubQueryExpression">SubQueryExpressions</see> are cloned, and their references also replaces. All referenced clauses must be mapped
  /// to cloned clauses in the given <see cref="QuerySourceMapping"/>, otherwise an expression is thrown. This is used by <see cref="QueryModel.Clone()"/>
  /// to adjust references to the old <see cref="QueryModel"/> with references to the new <see cref="QueryModel"/>.
  /// </summary>
  public class CloningExpressionTreeVisitor : ReferenceReplacingExpressionTreeVisitor
  {
    /// <summary>
    /// Adjusts the given expression for cloning, that is replaces <see cref="QuerySourceReferenceExpression"/> and <see cref="SubQueryExpression"/> 
    /// instances. All referenced clauses must be mapped to clones in the given <paramref name="querySourceMapping"/>, otherwise an exception is thrown.
    /// </summary>
    /// <param name="expression">The expression to be adjusted.</param>
    /// <param name="querySourceMapping">The clause mapping to be used for replacing <see cref="QuerySourceReferenceExpression"/> instances.</param>
    /// <returns>An expression with all <see cref="QuerySourceReferenceExpression"/> and <see cref="SubQueryExpression"/> instances replaced
    /// as required by a <see cref="QueryModel.Clone()"/> operation.</returns>
    public static Expression AdjustExpressionAfterCloning (Expression expression, QuerySourceMapping querySourceMapping)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("querySourceMapping", querySourceMapping);

      return new CloningExpressionTreeVisitor (querySourceMapping, false).VisitExpression (expression);
    }
    
    private CloningExpressionTreeVisitor (QuerySourceMapping querySourceMapping, bool ignoreUnmappedReferences)
      : base (querySourceMapping, ignoreUnmappedReferences)
    {
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var clonedQueryModel = expression.QueryModel.Clone (QuerySourceMapping);
      return new SubQueryExpression (clonedQueryModel);
    }
  }
}
