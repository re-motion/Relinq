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
  public sealed class CloningExpressionVisitor : RelinqExpressionVisitor
  {
    public static Expression AdjustExpressionAfterCloning (Expression expression, QuerySourceMapping querySourceMapping)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("querySourceMapping", querySourceMapping);

      return new CloningExpressionVisitor (querySourceMapping).Visit (expression);
    }

    private readonly QuerySourceMapping _querySourceMapping;

    private CloningExpressionVisitor (QuerySourceMapping querySourceMapping)
    {
      _querySourceMapping = querySourceMapping;
    }

    protected internal override Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (_querySourceMapping.ContainsMapping (expression.ReferencedQuerySource))
        return _querySourceMapping.GetExpression (expression.ReferencedQuerySource);

      return expression;
    }

    protected internal override Expression VisitSubQuery (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var clonedQueryModel = expression.QueryModel.Clone (_querySourceMapping);
      return new SubQueryExpression (clonedQueryModel);
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
