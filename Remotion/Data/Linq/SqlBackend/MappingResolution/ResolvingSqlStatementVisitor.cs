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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="ResolvingSqlStatementVisitor"/> provides methods to visit sql-statement classes.
  /// </summary>
  public class ResolvingSqlStatementVisitor
  {
    private readonly ISqlStatementResolver _resolver;
    private readonly UniqueIdentifierGenerator _uniqueIdentifierGenerator;

    public static void ResolveExpressions (
        SqlStatement statement, ISqlStatementResolver resolver, UniqueIdentifierGenerator uniqueIdentifierGenerator)
    {
      ArgumentUtility.CheckNotNull ("statement", statement);

      var visitor = new ResolvingSqlStatementVisitor (resolver, uniqueIdentifierGenerator);
      visitor.VisitSqlStatement (statement);
    }

    protected ResolvingSqlStatementVisitor (ISqlStatementResolver resolver, UniqueIdentifierGenerator uniqueIdentifierGenerator)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("uniqueIdentifierGenerator", uniqueIdentifierGenerator);

      _resolver = resolver;
      _uniqueIdentifierGenerator = uniqueIdentifierGenerator;
    }

    protected Expression VisitSelectProjection (Expression selectProjection)
    {
      ArgumentUtility.CheckNotNull ("selectProjection", selectProjection);

      return ResolvingExpressionVisitor.ResolveExpression (selectProjection, _resolver, _uniqueIdentifierGenerator);
    }

    protected void VisitSqlTable (SqlTable sqlTable)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      sqlTable.TableInfo = ResolvingTableInfoVisitor.ResolveTableInfo (sqlTable.TableInfo, _resolver, _uniqueIdentifierGenerator);
      ResolveJoins (sqlTable);
    }

    protected Expression VisitWhereCondition (Expression whereCondition)
    {
      ArgumentUtility.CheckNotNull ("whereCondition", whereCondition);

      return ResolvingExpressionVisitor.ResolveExpression (whereCondition, _resolver, _uniqueIdentifierGenerator);
    }

    protected Expression VisitOrderingExpression (Expression orderByExpression)
    {
      ArgumentUtility.CheckNotNull ("orderByExpression", orderByExpression);

      return ResolvingExpressionVisitor.ResolveExpression (orderByExpression, _resolver, _uniqueIdentifierGenerator);
    }

    protected Expression VisitTopExpression (Expression topExpression)
    {
      ArgumentUtility.CheckNotNull ("topExpression", topExpression);

      return ResolvingExpressionVisitor.ResolveExpression (topExpression, _resolver, _uniqueIdentifierGenerator);
    }

    private void ResolveJoins (SqlTableBase sqlTable)
    {
      foreach (var joinedTable in sqlTable.JoinedTables)
      {
        joinedTable.JoinInfo = ResolvingJoinInfoVisitor.ResolveJoinInfo (sqlTable, joinedTable.JoinInfo, _resolver, _uniqueIdentifierGenerator);
        ResolveJoins (joinedTable);
      }
    }


    protected virtual void VisitSqlStatement (SqlStatement sqlStatement)
    {
      foreach (var sqlTable in sqlStatement.SqlTables)
        VisitSqlTable (sqlTable);

      sqlStatement.SelectProjection = VisitSelectProjection (sqlStatement.SelectProjection);

      if (sqlStatement.WhereCondition != null)
        sqlStatement.WhereCondition = VisitWhereCondition (sqlStatement.WhereCondition);

      if (sqlStatement.TopExpression != null)
        sqlStatement.TopExpression = VisitTopExpression (sqlStatement.TopExpression);

      if (sqlStatement.Orderings.Count > 0)
      {
        foreach (var orderByClause in sqlStatement.Orderings)
          orderByClause.Expression = VisitOrderingExpression (orderByClause.Expression);
      }
    }
  }
}