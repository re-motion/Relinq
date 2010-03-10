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
using Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallGenerators;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlStatementTextGenerator"/> generates sql-text from a given <see cref="SqlStatement"/>.
  /// </summary>
  public class SqlStatementTextGenerator
  {
    private readonly MethodCallSqlGeneratorRegistry _registry;

    public SqlStatementTextGenerator ()
    {
// ReSharper disable DoNotCallOverridableMethodsInConstructor
      _registry = GenerateSqlGeneratorRegistry();
// ReSharper restore DoNotCallOverridableMethodsInConstructor
    }

    public SqlCommand Build (SqlStatement sqlStatement)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      
      var commandBuilder = new SqlCommandBuilder();
      commandBuilder.Append ("SELECT ");
      BuildSelectPart (sqlStatement, commandBuilder);
      commandBuilder.Append (" FROM ");
      BuildFromPart (sqlStatement, commandBuilder);
      if ((sqlStatement.WhereCondition != null))
      {
        commandBuilder.Append (" WHERE ");
        BuildWherePart (sqlStatement, commandBuilder);
      }

      return new SqlCommand (commandBuilder.GetCommandText(), commandBuilder.GetCommandParameters());
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
          SqlGeneratingExpressionVisitor.GenerateSql (sqlStatement.TopExpression, commandBuilder, _registry);
          commandBuilder.Append (") ");
        }
        SqlGeneratingExpressionVisitor.GenerateSql (sqlStatement.SelectProjection, commandBuilder, _registry);
      }
    }

    protected virtual void BuildFromPart (SqlStatement sqlStatement, SqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      SqlTableAndJoinTextGenerator.GenerateSql (sqlStatement.FromExpression, commandBuilder);
    }

    protected virtual void BuildWherePart (SqlStatement sqlStatement, SqlCommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      SqlGeneratingExpressionVisitor.GenerateSql (sqlStatement.WhereCondition, commandBuilder, _registry);
    }

    protected virtual MethodCallSqlGeneratorRegistry GenerateSqlGeneratorRegistry ()
    {
      var registry = new MethodCallSqlGeneratorRegistry();

      //TODO: Convert methods with all overloads needed
      var containsMethod = typeof (string).GetMethod ("Contains", new Type[] { typeof (string) });
      var convertToStringMethod = typeof (Convert).GetMethod ("ToString", new[] { typeof (int) });
      var convertToBoolMethod = typeof (Convert).GetMethod ("ToBoolean", new[] { typeof (int) });
      var convertToInt64Method = typeof (Convert).GetMethod ("ToInt64", new[] { typeof (int) });
      var convertToDateTimeMethod = typeof (Convert).GetMethod ("ToDateTime", new[] { typeof (int) });
      var convertToDoubleMethod = typeof (Convert).GetMethod ("ToDouble", new[] { typeof (int) });
      var convertToIntMethod = typeof (Convert).GetMethod ("ToInt32", new[] { typeof (int) });
      var convertToDecimalMethod = typeof (Convert).GetMethod ("ToDecimal", new[] { typeof (int) });
      var convertToCharMethod = typeof (Convert).GetMethod ("ToChar", new[] { typeof (int) });
      var convertToByteMethod = typeof (Convert).GetMethod ("ToByte", new[] { typeof (int) });
      var endsWithMethod = typeof (string).GetMethod ("EndsWith", new Type[] { typeof (string) });
      var lowerMethod = typeof (string).GetMethod ("ToLower", new Type[] { });
      var removeMethod = typeof (string).GetMethod ("Remove", new Type[] { typeof (int) });
      var startsWithMethod = typeof (string).GetMethod ("StartsWith", new Type[] { typeof (string) });
      var substringMethod = typeof (string).GetMethod ("Substring", new Type[] { typeof (int), typeof (int) });
      var toUpperMethod = typeof (string).GetMethod ("ToUpper", new Type[] { });

      registry.Register (containsMethod, new MethodCallContains());
      registry.Register (convertToStringMethod, new MethodCallConvert());
      registry.Register (convertToBoolMethod, new MethodCallConvert());
      registry.Register (convertToInt64Method, new MethodCallConvert());
      registry.Register (convertToDateTimeMethod, new MethodCallConvert());
      registry.Register (convertToDoubleMethod, new MethodCallConvert());
      registry.Register (convertToIntMethod, new MethodCallConvert());
      registry.Register (convertToDecimalMethod, new MethodCallConvert());
      registry.Register (convertToCharMethod, new MethodCallConvert());
      registry.Register (convertToByteMethod, new MethodCallConvert());
      registry.Register (endsWithMethod, new MethodCallEndsWith());
      registry.Register (lowerMethod, new MethodCallLower());
      registry.Register (removeMethod, new MethodCallRemove ());
      registry.Register (startsWithMethod, new MethodCallStartsWith());
      registry.Register (substringMethod, new MethodCallSubstring());
      registry.Register (toUpperMethod, new MethodCallUpper());

      return registry;
    }
  }
}