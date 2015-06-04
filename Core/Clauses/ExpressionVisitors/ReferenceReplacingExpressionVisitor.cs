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
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ExpressionVisitors
{
  /// <summary>
  /// Takes an expression and replaces all <see cref="QuerySourceReferenceExpression"/> instances, as defined by a given <see cref="QuerySourceMapping"/>.
  /// This is used whenever references to query sources should be replaced by a transformation.
  /// </summary>
  public sealed class ReferenceReplacingExpressionVisitor : RelinqExpressionVisitor
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

      return new ReferenceReplacingExpressionVisitor (querySourceMapping, throwOnUnmappedReferences).Visit (expression);
    }

    private readonly QuerySourceMapping _querySourceMapping;
    private readonly bool _throwOnUnmappedReferences;

    private ReferenceReplacingExpressionVisitor (QuerySourceMapping querySourceMapping, bool throwOnUnmappedReferences)
    {
      ArgumentUtility.CheckNotNull ("querySourceMapping", querySourceMapping);
      _querySourceMapping = querySourceMapping;
      _throwOnUnmappedReferences = throwOnUnmappedReferences;
    }

    protected internal override Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (_querySourceMapping.ContainsMapping (expression.ReferencedQuerySource))
      {
        return _querySourceMapping.GetExpression (expression.ReferencedQuerySource);
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

    protected internal override Expression VisitSubQuery (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      expression.QueryModel.TransformExpressions (ex => ReplaceClauseReferences (ex, _querySourceMapping, _throwOnUnmappedReferences));
      return expression;
    }

#if NET_3_5
    protected override Expression VisitRelinqUnknownNonExtension (Expression expression)
    {
      //ignore
      return expression;
    }
#endif
  }

}
