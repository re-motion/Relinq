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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlTableAndJoinTextGenerator"/> generates sql-text for <see cref="ResolvedSimpleTableInfo"/> and <see cref="ResolvedJoinInfo"/>.
  /// </summary>
  public class SqlTableAndJoinTextGenerator : ITableInfoVisitor, IJoinInfoVisitor
  {
    private readonly SqlCommandBuilder _commandBuilder;
    private readonly ISqlGenerationStage _stage;
    private bool _first;
    
    public static void GenerateSql (SqlTableBase sqlTable, SqlCommandBuilder commandBuilder, ISqlGenerationStage stage, bool first)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlTableAndJoinTextGenerator (commandBuilder, stage, first);

      sqlTable.GetResolvedTableInfo().Accept (visitor);
      GenerateSqlForJoins (sqlTable, visitor);
    }

    private static void GenerateSqlForJoins (SqlTableBase sqlTable, SqlTableAndJoinTextGenerator visitor)
    {
      foreach (var joinedTable in sqlTable.JoinedTables)
      {
        joinedTable.JoinInfo.Accept (visitor);
        GenerateSqlForJoins (joinedTable, visitor);
      }
    }

    protected SqlTableAndJoinTextGenerator (SqlCommandBuilder commandBuilder, ISqlGenerationStage stage, bool first)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("stage", stage);

      _commandBuilder = commandBuilder;
      _stage = stage;
      _first = first;
    }

    public AbstractTableInfo VisitUnresolvedTableInfo (UnresolvedTableInfo tableInfo)
    {
      throw new InvalidOperationException ("UnresolvedTableInfo is not valid at this point.");
    }

    public AbstractTableInfo VisitSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      if (!_first)
      {
        _commandBuilder.Append (" CROSS JOIN ");
        _first = true;
      }
      
      _commandBuilder.Append ("[");
      _commandBuilder.Append (tableInfo.TableName);
      _commandBuilder.Append ("]");
      _commandBuilder.Append (" AS ");
      _commandBuilder.Append ("[");
      _commandBuilder.Append (tableInfo.TableAlias);
      _commandBuilder.Append ("]");

      return tableInfo;
    }

    public AbstractTableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo tableInfo)
    {
      if (!_first)
      {
        _commandBuilder.Append (" CROSS APPLY ");
      }

      _commandBuilder.Append ("(");
      _stage.GenerateTextForSqlStatement (_commandBuilder, tableInfo.SqlStatement);
      _commandBuilder.Append (")");
      _commandBuilder.Append (" AS ");
      _commandBuilder.Append ("[");
      _commandBuilder.Append (tableInfo.TableAlias);
      _commandBuilder.Append ("]");
      return tableInfo;
    }

    public AbstractJoinInfo VisitResolvedJoinInfo (ResolvedJoinInfo tableSource)
    {
      _commandBuilder.Append (" LEFT OUTER JOIN ");

      ((AbstractTableInfo) tableSource.ForeignTableInfo).Accept (this);

      _commandBuilder.Append (" ON ");

      _stage.GenerateTextForJoinKeyExpression (_commandBuilder, tableSource.PrimaryColumn);
      _commandBuilder.Append (" = ");
      _stage.GenerateTextForJoinKeyExpression (_commandBuilder, tableSource.ForeignColumn);
      return tableSource;
    }

    public AbstractJoinInfo VisitUnresolvedJoinInfo (UnresolvedJoinInfo tableSource)
    {
      throw new InvalidOperationException ("UnresolvedJoinInfo is not valid at this point.");
    }
  }
}