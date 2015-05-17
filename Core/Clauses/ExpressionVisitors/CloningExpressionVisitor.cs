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
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ExpressionVisitors
{
  /// <summary>
  /// Visits an <see cref="Expression"/> tree, replacing all <see cref="QuerySourceReferenceExpression"/> instances with references to cloned clauses,
  /// as defined by a <see cref="QuerySourceMapping"/>. In addition, all <see cref="QueryModel"/> instances in 
  /// <see cref="SubQueryExpression">SubQueryExpressions</see> are cloned, and their references also replaces. All referenced clauses must be mapped
  /// to cloned clauses in the given <see cref="QuerySourceMapping"/>, otherwise an expression is thrown. This is used by <see cref="QueryModel.Clone()"/>
  /// to adjust references to the old <see cref="QueryModel"/> with references to the new <see cref="QueryModel"/>.
  /// </summary>
  public class CloningExpressionVisitor : ReferenceReplacingExpressionVisitor
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

      return new CloningExpressionVisitor (querySourceMapping, false).Visit (expression);
    }
    
    private CloningExpressionVisitor (QuerySourceMapping querySourceMapping, bool ignoreUnmappedReferences)
      : base (querySourceMapping, ignoreUnmappedReferences)
    {
    }

    protected internal override Expression VisitSubQuery (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var clonedQueryModel = expression.QueryModel.Clone (QuerySourceMapping);
      return new SubQueryExpression (clonedQueryModel);
    }
  }
}
