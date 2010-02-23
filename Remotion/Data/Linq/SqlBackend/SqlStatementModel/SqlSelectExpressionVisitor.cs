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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlSelectExpressionVisitor"/> transforms <see cref="SqlStatement.SelectProjection"/> 
  /// to a <see cref="SqlTableReferenceExpression"/>.
  /// </summary>
  public class SqlSelectExpressionVisitor : ThrowingExpressionTreeVisitor
  {
    private readonly SqlGenerationContext _context;

    public static SqlTableReferenceExpression TranslateSelectExpression (Expression projection, SqlGenerationContext context)
    {
      var visitor = new SqlSelectExpressionVisitor (context);
      var result = visitor.VisitExpression (projection);
      return (SqlTableReferenceExpression) result;
    }

    protected SqlSelectExpressionVisitor (SqlGenerationContext context)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      _context = context;
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      return new SqlTableReferenceExpression (expression.Type, _context.GetSqlTableExpression(expression.ReferencedQuerySource));
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      var message = string.Format (
         "The given expression type '{0}' is not supported in select clauses. (Expression: '{1}')",
         unhandledItem.GetType().Name,
         unhandledItem);
      throw new NotSupportedException (message);
    }
  }
}