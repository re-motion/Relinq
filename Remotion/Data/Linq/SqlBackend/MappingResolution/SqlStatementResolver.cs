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
  /// <see cref="SqlStatementResolver"/> provides methods to visit sql-statement classes.
  /// </summary>
  public class SqlStatementResolver
  {
    private readonly IMappingResolutionStage _stage;

    public static void ResolveExpressions (IMappingResolutionStage stage, SqlStatement statement)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("statement", statement);
      
      var visitor = new SqlStatementResolver (stage);
      visitor.VisitSqlStatement (statement);
    }

    protected SqlStatementResolver (IMappingResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      
      _stage = stage;
    }

    protected Expression VisitSelectProjection (Expression selectProjection)
    {
      ArgumentUtility.CheckNotNull ("selectProjection", selectProjection);

      return _stage.ResolveSelectExpression (selectProjection);
    }

    protected void VisitSqlTable (SqlTable sqlTable)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      sqlTable.TableInfo = _stage.ResolveTableInfo (sqlTable.TableInfo);
      ResolveJoins (sqlTable);
    }

    protected Expression VisitWhereCondition (Expression whereCondition)
    {
      ArgumentUtility.CheckNotNull ("whereCondition", whereCondition);

      return _stage.ResolveWhereExpression (whereCondition);
    }

    protected Expression VisitOrderingExpression (Expression orderByExpression)
    {
      ArgumentUtility.CheckNotNull ("orderByExpression", orderByExpression);

      return _stage.ResolveOrderingExpression (orderByExpression);
    }

    protected Expression VisitTopExpression (Expression topExpression)
    {
      ArgumentUtility.CheckNotNull ("topExpression", topExpression);

      return _stage.ResolveTopExpression (topExpression);
    }

    private void ResolveJoins (SqlTableBase sqlTable)
    {
      foreach (var joinedTable in sqlTable.JoinedTables)
      {
        joinedTable.JoinInfo = _stage.ResolveJoinInfo (sqlTable, joinedTable.JoinInfo);
        ResolveJoins (joinedTable);
      }
    }

    protected virtual void VisitSqlStatement (SqlStatement sqlStatement)
    {
      foreach (var sqlTable in sqlStatement.SqlTables)
        VisitSqlTable (sqlTable);

      sqlStatement.SelectProjection = _stage.ResolveSelectExpression (sqlStatement.SelectProjection);

      if (sqlStatement.WhereCondition != null)
        sqlStatement.WhereCondition = _stage.ResolveWhereExpression (sqlStatement.WhereCondition);

      if (sqlStatement.TopExpression != null)
        sqlStatement.TopExpression = _stage.ResolveTopExpression (sqlStatement.TopExpression);

      if (sqlStatement.Orderings.Count > 0)
      {
        foreach (var orderByClause in sqlStatement.Orderings)
          orderByClause.Expression = _stage.ResolveOrderingExpression (orderByClause.Expression);
      }
    }
  }
}