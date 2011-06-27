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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors
{
  /// <summary>
  /// Replaces <see cref="Expression"/> nodes according to a given mapping specification. Expressions are also replaced within subqueries; the 
  /// <see cref="QueryModel"/> is changed by the replacement operations, it is not copied. The replacement node is not recursively searched for 
  /// occurrences of <see cref="Expression"/> nodes to be replaced.
  /// </summary>
  public class MultiReplacingExpressionTreeVisitor : ExpressionTreeVisitor
  {
    private readonly IDictionary<Expression, Expression> _expressionMapping;

    public static Expression Replace (IDictionary<Expression, Expression> expressionMapping, Expression sourceTree)
    {
      ArgumentUtility.CheckNotNull ("expressionMapping", expressionMapping);
      ArgumentUtility.CheckNotNull ("sourceTree", sourceTree);

      var visitor = new MultiReplacingExpressionTreeVisitor (expressionMapping);
      return visitor.VisitExpression (sourceTree);
    }

    private MultiReplacingExpressionTreeVisitor (IDictionary<Expression, Expression> expressionMapping)
    {
      ArgumentUtility.CheckNotNull ("expressionMapping", expressionMapping);

      _expressionMapping = expressionMapping;
    }

    public override Expression VisitExpression (Expression expression)
    {
      Expression replacementExpression;
      if (expression != null && _expressionMapping.TryGetValue (expression, out replacementExpression))
        return replacementExpression;
      else
        return base.VisitExpression (expression);
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      expression.QueryModel.TransformExpressions (VisitExpression);
      return expression; // Note that we modifiy the (mutable) QueryModel, we return an unchanged expression
    }

    protected internal override Expression VisitUnknownNonExtensionExpression (Expression expression)
    {
      //ignore
      return expression;
    }
  }
}