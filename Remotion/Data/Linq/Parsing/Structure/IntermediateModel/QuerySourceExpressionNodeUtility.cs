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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Provides common functionality used by implementors of <see cref="IQuerySourceExpressionNode"/>.
  /// </summary>
  public class QuerySourceExpressionNodeUtility
  {
    /// <summary>
    /// Replaces the given parameter with a back-reference to the <see cref="IQuerySourceWithItemType"/> corresponding to <paramref name="referencedNode"/>.
    /// </summary>
    /// <param name="referencedNode">The referenced node.</param>
    /// <param name="parameterToReplace">The parameter to replace with a <see cref="QuerySourceReferenceExpression"/>.</param>
    /// <param name="expression">The expression in which to replace the parameter.</param>
    /// <param name="querySourceClauseMapping">The query source clause mapping.</param>
    /// <returns><paramref name="expression"/>, with <paramref name="parameterToReplace"/> replaced with a <see cref="QuerySourceReferenceExpression"/>
    /// pointing to the clause corresponding to <paramref name="referencedNode"/>.</returns>
    public static Expression ReplaceParameterWithReference (
        IQuerySourceExpressionNode referencedNode, 
        ParameterExpression parameterToReplace, 
        Expression expression, 
        QuerySourceClauseMapping querySourceClauseMapping)
    {
      ArgumentUtility.CheckNotNull ("referencedNode", referencedNode);
      ArgumentUtility.CheckNotNull ("parameterToReplace", parameterToReplace);
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("querySourceClauseMapping", querySourceClauseMapping);

      var clause = GetQuerySourceForNode (referencedNode, querySourceClauseMapping);
      var referenceExpression = new QuerySourceReferenceExpression (clause);

      return ReplacingExpressionTreeVisitor.Replace (parameterToReplace, referenceExpression, expression);
    }

    /// <summary>
    /// Gets the <see cref="IQuerySourceWithItemType"/> corresponding to the given <paramref name="node"/>, throwing an <see cref="InvalidOperationException"/>
    /// if no such clause has been registered in the given <paramref name="querySourceClauseMapping"/>.
    /// </summary>
    /// <param name="node">The node for which the <see cref="IQuerySourceWithItemType"/> should be returned.</param>
    /// <param name="querySourceClauseMapping">The query source clause mapping.</param>
    /// <returns>The <see cref="IQuerySourceWithItemType"/> corresponding to <paramref name="node"/>.</returns>
    public static IQuerySourceWithItemType GetQuerySourceForNode (IQuerySourceExpressionNode node, QuerySourceClauseMapping querySourceClauseMapping)
    {
      try
      {
        return querySourceClauseMapping.GetClause (node);
      }
      catch (KeyNotFoundException ex)
      {
        var message = string.Format (
            "Cannot retrieve an IQuerySource for the given {0}. Be sure to call Apply before calling methods that require IQuerySources, and pass in "
            + "the same QuerySourceClauseMapping to both.",
            node.GetType().Name);
        throw new InvalidOperationException (message, ex);
      }
    }
  }
}