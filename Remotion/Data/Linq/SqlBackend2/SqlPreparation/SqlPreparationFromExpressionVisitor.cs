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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Analyzes the <see cref="FromClauseBase.FromExpression"/> of a <see cref="FromClauseBase"/> and returns a <see cref="SqlTableBase"/> that 
  /// represents the data source of the <see cref="FromClauseBase"/>.
  /// </summary>
  public class SqlPreparationFromExpressionVisitor : ThrowingExpressionTreeVisitor, IUnresolvedSqlExpressionVisitor, ISqlSubStatementExpressionVisitor
  {
    public static SqlTableBase GetTableForFromExpression (Expression fromExpression, Type itemType, ISqlPreparationStage stage, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("fromExpression", fromExpression);
      ArgumentUtility.CheckNotNull ("itemType", itemType);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("generator", generator);

      var visitor = new SqlPreparationFromExpressionVisitor(itemType, generator);
      var result = (SqlTableReferenceExpression) visitor.VisitExpression (fromExpression);
      return result.SqlTable;
    }

    private readonly Type _itemType;
    private readonly UniqueIdentifierGenerator _generator;

    protected SqlPreparationFromExpressionVisitor (Type itemType, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("itemType", itemType);
      ArgumentUtility.CheckNotNull ("generator", generator);

      _itemType = itemType;
      _generator = generator;
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var sqlTable = new SqlTable (new UnresolvedTableInfo (_itemType));
      return new SqlTableReferenceExpression (sqlTable);
    }

    public Expression VisitSqlMemberExpression (SqlMemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      
      var joinedTable = new SqlJoinedTable (new UnresolvedJoinInfo(expression.SqlTable, expression.MemberInfo, JoinCardinality.Many));

      return new SqlTableReferenceExpression (joinedTable);
    }

    public Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      // Note: This is a case where the SQL preparation stage already generates a resolved table info (including a table alias) rather than passing
      // on an unresolved table info to the mapping resolution stage. Should we ever have the need to resolve subqueries in the mapping resolution 
      // stage, we should refactor this into an UnresolvedSubStatemenTableInfo.
      
      var type = ReflectionUtility.GetItemTypeOfIEnumerable (expression.Type, "expression");
      var tableInfo = new ResolvedSubStatementTableInfo (type, _generator.GetUniqueIdentifier ("q"), expression.SqlStatement);
      var sqlTable = new SqlTable (tableInfo);
      return new SqlTableReferenceExpression (sqlTable);
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      ArgumentUtility.CheckNotNull ("unhandledItem", unhandledItem);
      ArgumentUtility.CheckNotNullOrEmpty ("visitMethod", visitMethod);

      var message = string.Format ("Expressions of type '{0}' cannot be used as the SqlTables of a from clause.", unhandledItem.GetType().Name);
      return new NotSupportedException (message);
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      return base.VisitUnknownExpression (expression);
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityRefMemberExpression (SqlEntityRefMemberExpression expression)
    {
      return base.VisitUnknownExpression (expression);
    }

  }
}