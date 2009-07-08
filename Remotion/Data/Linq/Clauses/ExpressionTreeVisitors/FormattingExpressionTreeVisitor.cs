// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ExpressionTreeVisitors
{
  /// <summary>
  /// Transforms an expression tree into a human-readable string, taking all the custom expression nodes into account.
  /// It does so by replacing all instances of custom expression nodes by parameters that have the desired string as their names. This is done
  /// to circumvent a limitation in the <see cref="Expression"/> class, where overriding <see cref="Expression.ToString"/> in custom expressions
  /// will not work.
  /// </summary>
  public class FormattingExpressionTreeVisitor : ExpressionTreeVisitor
  {
    public static string Format (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      var transformedExpression = new FormattingExpressionTreeVisitor().VisitExpression (expression);
      return transformedExpression.ToString();
    }

    private FormattingExpressionTreeVisitor ()
    {
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return Expression.Parameter (expression.Type, "[" + expression.ReferencedClause.ItemName + "]");
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return Expression.Parameter (expression.Type, "{" + expression.QueryModel + "}");
    }

    protected override Expression VisitUnknownExpression (Expression expression)
    {
      //ignore
      return expression;
    }
  }
}