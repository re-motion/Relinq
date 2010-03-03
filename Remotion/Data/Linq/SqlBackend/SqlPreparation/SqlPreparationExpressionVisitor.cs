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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlPreparationExpressionVisitor"/> transforms the expressions stored by <see cref="SqlStatement.SelectProjection"/> to a SQL-specific
  /// format.
  /// </summary>
  // TODO: Sql preparation phase should ignore expression types that don't need to be prepared. Therefore, derive from ExpressionTreeVisitor instead.
  public class SqlPreparationExpressionVisitor : ThrowingExpressionTreeVisitor
  { 
    private readonly SqlPreparationContext _context;

    public static Expression TranslateExpression (Expression projection, SqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("projection", projection);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new SqlPreparationExpressionVisitor (context);
      var result = visitor.VisitExpression (projection);
      return result;
    }

    protected SqlPreparationExpressionVisitor (SqlPreparationContext context)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      _context = context;
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      
      var referencedTable = _context.GetSqlTableForQuerySource (expression.ReferencedQuerySource);
      return new SqlTableReferenceExpression (referencedTable);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // First process any nested expressions
      // E.g, for (kitchen.Cook).FirstName, first process kitchen => newExpression1 (SqlTableReferenceExpression)
      // then newExpression1.Cook => newExpression2 (SqlMemberExpression)
      // then newExpression2.FirstName => result (SqlMemberExpression)
      var newExpression = VisitExpression (expression.Expression);
      
      // kitchen case: newExpression is a SqlTableReferenceExpression (kitchenTable)
      // create a SqlMemberExpression (kitchenTable, "Cook")
      var newExpressionAsTableReference = newExpression as SqlTableReferenceExpression;
      if (newExpressionAsTableReference != null)
      {
        var sqlTable = newExpressionAsTableReference.SqlTable; // kitchenTable
        return new SqlMemberExpression (sqlTable, expression.Member); // kitchenTable.Cook
      }

      // kitchen.Cook case: newExpression is a SqlMemberExpression (kitchenTable, "Cook")
      // create a join kitchenTable => cookTable via "Cook" member
      // create a SqlMemberExpression (cookTable, "FirstName")
      var newExpressionAsSqlMemberExpression = newExpression as SqlMemberExpression;
      if (newExpressionAsSqlMemberExpression != null)
      {
        var originalSqlTable = newExpressionAsSqlMemberExpression.SqlTable; // kitchenTable
        
        // create cookTable via join
        var join = originalSqlTable.GetOrAddJoin (
            newExpressionAsSqlMemberExpression.MemberInfo, // "Cook"
            new JoinedTableSource (newExpressionAsSqlMemberExpression.MemberInfo));

        return new SqlMemberExpression (originalSqlTable, expression.Member); // cookTable.FirstName // TODO: Use join, not originalSqlTable.
      }
      return expression;
    }

    // TODO: Remove
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