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

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors
{
  /// <summary>
  /// Replaces all nodes that equal a given <see cref="Expression"/> with a replacement node. Expressions are also replaced within subqueries; the 
  /// <see cref="QueryModel"/> is changed by the replacement operations, it is not copied. The replacement node is not recursively searched for 
  /// occurrences of the <see cref="Expression"/> to be replaced.
  /// </summary>
  public class ReplacingExpressionTreeVisitor : ExpressionTreeVisitor
  {
    public static Expression Replace (Expression replacedExpression, Expression replacementExpression, Expression sourceTree)
    {
      ArgumentUtility.CheckNotNull ("replacedExpression", replacedExpression);
      ArgumentUtility.CheckNotNull ("replacementExpression", replacementExpression);
      ArgumentUtility.CheckNotNull ("sourceTree", sourceTree);

      var visitor = new ReplacingExpressionTreeVisitor (replacedExpression, replacementExpression);
      return visitor.VisitExpression (sourceTree);
    }

    private readonly Expression _replacedExpression;
    private readonly Expression _replacementExpression;

    private ReplacingExpressionTreeVisitor (Expression replacedExpression, Expression replacementExpression)
    {
      _replacedExpression = replacedExpression;
      _replacementExpression = replacementExpression;
    }

    public override Expression VisitExpression (Expression expression)
    {
      if (Equals (expression, _replacedExpression))
        return _replacementExpression;
      else
        return base.VisitExpression (expression);
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      expression.QueryModel.TransformExpressions (VisitExpression);
      return expression; // Note that we modifiy the (mutable) QueryModel, we return an unchanged expression
    }

    protected override Expression VisitUnknownNonExtension (Expression expression)
    {
      //ignore
      return expression;
    }
  }
}