// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Linq.Expressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors
{
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
      if (expression == _replacedExpression)
        return _replacementExpression;
      else
        return base.VisitExpression (expression);
    }

    protected internal override Expression VisitUnknownExpression (Expression expression)
    {
      //ignore
      return expression;
    }
  }
}
