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
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Utilities;

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

    protected internal override Expression VisitUnknownNonExtensionExpression (Expression expression)
    {
      //ignore
      return expression;
    }
  }
}