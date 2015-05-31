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
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.ExpressionVisitors;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Provides common functionality used by implementors of <see cref="IQuerySourceExpressionNode"/>.
  /// </summary>
  public static class QuerySourceExpressionNodeUtility
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

      return ReplacingExpressionVisitor.Replace (parameterToReplace, referenceExpression, expression);
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
