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
  public class SqlPreparationExpressionVisitor : ExpressionTreeVisitor
  { 
    private readonly SqlPreparationContext _context;
    private readonly ISqlPreparationStage _stage;

    public static Expression TranslateExpression (Expression projection, SqlPreparationContext context, ISqlPreparationStage stage)
    {
      ArgumentUtility.CheckNotNull ("projection", projection);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlPreparationExpressionVisitor (context, stage);
      var result = visitor.VisitExpression (projection);
      return result;
    }

    protected SqlPreparationExpressionVisitor (SqlPreparationContext context, ISqlPreparationStage stage)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);
      
      _context = context;
      _stage = stage;
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
        var join = originalSqlTable.GetOrAddJoin (newExpressionAsSqlMemberExpression.MemberInfo, JoinCardinality.One); // "Cook"

        return new SqlMemberExpression (join, expression.Member); // cookTable.FirstName
      }
      return expression;
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      var sqlStatement = _stage.PrepareSqlStatement (expression.QueryModel);
      return new SqlSubStatementExpression (sqlStatement, expression.Type);
    }
  }
}