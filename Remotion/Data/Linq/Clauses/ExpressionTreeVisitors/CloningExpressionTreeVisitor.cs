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

namespace Remotion.Data.Linq.Clauses.ExpressionTreeVisitors
{
  /// <summary>
  /// Visits an <see cref="Expression"/> tree, replacing all <see cref="QuerySourceReferenceExpression"/> instances with references to cloned clauses,
  /// as defined by a <see cref="ClauseMapping"/>. In addition, all <see cref="QueryModel"/> instances in 
  /// <see cref="SubQueryExpression">SubQueryExpressions</see> are cloned, and their references also replaces. All referenced clauses must be mapped
  /// to cloned clauses in the given <see cref="ClauseMapping"/>, otherwise an expression is thrown. This is used by <see cref="QueryModel.Clone()"/>
  /// to adjust references to the old <see cref="QueryModel"/> with references to the new <see cref="QueryModel"/>.
  /// </summary>
  public class CloningExpressionTreeVisitor : ReferenceReplacingExpressionTreeVisitor
  {
    /// <summary>
    /// Adjusts the given expression for cloning, that is replaces <see cref="QuerySourceReferenceExpression"/> and <see cref="SubQueryExpression"/> 
    /// instances. All referenced clauses must be mapped to clones in the given <paramref name="clauseMapping"/>, otherwise an exception is thrown.
    /// </summary>
    /// <param name="expression">The expression to be adjusted.</param>
    /// <param name="clauseMapping">The clause mapping to be used for replacing <see cref="QuerySourceReferenceExpression"/> instances.</param>
    /// <returns>An expression with all <see cref="QuerySourceReferenceExpression"/> and <see cref="SubQueryExpression"/> instances replaced
    /// as required by a <see cref="QueryModel.Clone()"/> operation.</returns>
    public static Expression AdjustExpressionAfterCloning (Expression expression, ClauseMapping clauseMapping)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("clauseMapping", clauseMapping);

      return new CloningExpressionTreeVisitor (clauseMapping, false).VisitExpression (expression);
    }
    
    private CloningExpressionTreeVisitor (ClauseMapping clauseMapping, bool ignoreUnmappedReferences)
      : base (clauseMapping, ignoreUnmappedReferences)
    {
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var clonedQueryModel = expression.QueryModel.Clone (ClauseMapping);
      return new SubQueryExpression (clonedQueryModel);
    }
  }
}