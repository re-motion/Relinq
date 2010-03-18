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

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlStatementTextGenerator"/> generates sql-text from a given <see cref="SqlStatement"/>.
  /// </summary>
  public class SqlStatementTextGenerator
  {
    private readonly ISqlGenerationStage _stage;

    public SqlStatementTextGenerator (ISqlGenerationStage stage)
    {
      ArgumentUtility.CheckNotNull ("stage", stage);
      
      _stage = stage;
    }

    public void Build (SqlStatement sqlStatement, SqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      
      commandBuilder.Append ("SELECT ");
      BuildSelectPart (sqlStatement, commandBuilder);
      commandBuilder.Append (" FROM ");
      BuildFromPart (sqlStatement, commandBuilder);
      if ((sqlStatement.WhereCondition != null))
      {
        commandBuilder.Append (" WHERE ");
        BuildWherePart (sqlStatement, commandBuilder);
      }
      if (sqlStatement.Orderings.Count > 0)
      {
        commandBuilder.Append (" ORDER BY ");
        BuildOrderByPart (sqlStatement, commandBuilder);
      }
    }

    protected virtual void BuildSelectPart (SqlStatement sqlStatement, SqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      if ((sqlStatement.IsCountQuery && sqlStatement.TopExpression != null) || (sqlStatement.IsCountQuery && sqlStatement.IsDistinctQuery))
        throw new NotSupportedException ("A SqlStatement cannot contain both Count and Top or Count and Distinct.");

      if (sqlStatement.IsCountQuery)
      {
        commandBuilder.Append ("COUNT(*)");
      }
      else
      {
        if (sqlStatement.IsDistinctQuery)
        {
          commandBuilder.Append ("DISTINCT ");
        }
        if (sqlStatement.TopExpression != null)
        {
          commandBuilder.Append ("TOP (");
          _stage.GenerateTextForTopExpression (commandBuilder, sqlStatement.TopExpression);
          commandBuilder.Append (") ");
        }
        _stage.GenerateTextForSelectExpression (commandBuilder, sqlStatement.SelectProjection);
      }
    }

    protected virtual void BuildFromPart (SqlStatement sqlStatement, SqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      //_stage.GenerateTextForFromTable (commandBuilder, sqlStatement.SqlTables);

      bool first = true;
      foreach (var sqlTable in sqlStatement.SqlTables)
      {
        _stage.GenerateTextForFromTable (commandBuilder, sqlTable, first);
        first = false;
      }
    }

    protected virtual void BuildWherePart (SqlStatement sqlStatement, SqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      _stage.GenerateTextForWhereExpression (commandBuilder, sqlStatement.WhereCondition);
    }

    protected virtual void BuildOrderByPart (SqlStatement sqlStatement, SqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      bool first = true;
      foreach (var orderByClause in sqlStatement.Orderings)
      {
        if (!first)
          commandBuilder.Append (", ");

        if (orderByClause.Expression.NodeType != ExpressionType.Constant)
        {
          _stage.GenerateTextForOrderByExpression (commandBuilder, orderByClause.Expression);
        }
        else
        {
          commandBuilder.Append ("(SELECT ");
          _stage.GenerateTextForOrderByExpression (commandBuilder, orderByClause.Expression);
          commandBuilder.Append (")");
        }
        commandBuilder.Append (string.Format (" {0}", orderByClause.OrderingDirection.ToString ().ToUpper ()));
        first = false;
      }
    }
  }
}