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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Provides common functionality used by implementors of <see cref="IQuerySourceExpressionNode"/>.
  /// </summary>
  public class QuerySourceExpressionNodeUtility
  {
    /// <summary>
    /// Replaces the given parameter with a back-reference to the <see cref="IQuerySource"/> corresponding to <paramref name="referencedNode"/>.
    /// </summary>
    /// <param name="referencedNode">The referenced node.</param>
    /// <param name="parameterToReplace">The parameter to replace with a <see cref="QuerySourceReferenceExpression"/>.</param>
    /// <param name="expression">The expression in which to replace the parameter.</param>
    /// <param name="context">The clause generation context.</param>
    /// <returns><paramref name="expression"/>, with <paramref name="parameterToReplace"/> replaced with a <see cref="QuerySourceReferenceExpression"/>
    /// pointing to the clause corresponding to <paramref name="referencedNode"/>.</returns>
    public static Expression ReplaceParameterWithReference (
        IQuerySourceExpressionNode referencedNode, 
        ParameterExpression parameterToReplace, 
        Expression expression, 
        ClauseGenerationContext context)
    {
      ArgumentUtility.CheckNotNull ("referencedNode", referencedNode);
      ArgumentUtility.CheckNotNull ("parameterToReplace", parameterToReplace);
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      var clause = GetQuerySourceForNode (referencedNode, context);
      var referenceExpression = new QuerySourceReferenceExpression (clause);

      return ReplacingExpressionTreeVisitor.Replace (parameterToReplace, referenceExpression, expression);
    }

    /// <summary>
    /// Gets the <see cref="IQuerySource"/> corresponding to the given <paramref name="node"/>, throwing an <see cref="InvalidOperationException"/>
    /// if no such clause has been registered in the given <paramref name="context"/>.
    /// </summary>
    /// <param name="node">The node for which the <see cref="IQuerySource"/> should be returned.</param>
    /// <param name="context">The clause generation context.</param>
    /// <returns>The <see cref="IQuerySource"/> corresponding to <paramref name="node"/>.</returns>
    public static IQuerySource GetQuerySourceForNode (IQuerySourceExpressionNode node, ClauseGenerationContext context)
    {
      try
      {
        return (IQuerySource) context.GetContextInfo (node);
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
