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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.ExpressionVisitors
{
  /// <summary>
  /// Replaces <see cref="Expression"/> nodes according to a given mapping specification. Expressions are also replaced within subqueries; the 
  /// <see cref="QueryModel"/> is changed by the replacement operations, it is not copied. The replacement node is not recursively searched for 
  /// occurrences of <see cref="Expression"/> nodes to be replaced.
  /// </summary>
  public sealed class MultiReplacingExpressionVisitor : RelinqExpressionVisitor
  {
    private readonly IDictionary<Expression, Expression> _expressionMapping;

    public static Expression Replace (IDictionary<Expression, Expression> expressionMapping, Expression sourceTree)
    {
      ArgumentUtility.CheckNotNull ("expressionMapping", expressionMapping);
      ArgumentUtility.CheckNotNull ("sourceTree", sourceTree);

      var visitor = new MultiReplacingExpressionVisitor (expressionMapping);
      return visitor.Visit (sourceTree);
    }

    private MultiReplacingExpressionVisitor (IDictionary<Expression, Expression> expressionMapping)
    {
      ArgumentUtility.CheckNotNull ("expressionMapping", expressionMapping);

      _expressionMapping = expressionMapping;
    }

    public override Expression Visit (Expression expression)
    {
      Expression replacementExpression;
      if (expression != null && _expressionMapping.TryGetValue (expression, out replacementExpression))
        return replacementExpression;
      else
        return base.Visit (expression);
    }

    protected internal override Expression VisitSubQuery (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      expression.QueryModel.TransformExpressions (Visit);
      return expression; // Note that we modifiy the (mutable) QueryModel, we return an unchanged expression
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