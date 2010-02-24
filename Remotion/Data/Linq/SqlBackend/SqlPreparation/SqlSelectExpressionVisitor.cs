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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlSelectExpressionVisitor"/> transforms the expressions stored by <see cref="SqlStatement.SelectProjection"/> to a SQL-specific
  /// format.
  /// </summary>
  public class SqlSelectExpressionVisitor : ThrowingExpressionTreeVisitor
  {
    private readonly SqlPreparationContext _context;

    // TODO: Change return type to Expression - in the future, we'll also support more complex expressions in select clauses.
    public static SqlTableReferenceExpression TranslateSelectExpression (Expression projection, SqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("projection", projection);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new SqlSelectExpressionVisitor (context);
      var result = visitor.VisitExpression (projection);
      return (SqlTableReferenceExpression) result;
    }

    protected SqlSelectExpressionVisitor (SqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      _context = context;
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return new SqlTableReferenceExpression (_context.GetSqlTableForQuerySource(expression.ReferencedQuerySource));
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      ArgumentUtility.CheckNotNull ("unhandledItem", unhandledItem);
      ArgumentUtility.CheckNotNullOrEmpty ("visitMethod", visitMethod);

      var message = string.Format (
          "The given expression type '{0}' is not supported in select clauses. (Expression: '{1}')",
          unhandledItem.GetType().Name,
          unhandledItem);
      throw new NotSupportedException (message);
    }
  }
}