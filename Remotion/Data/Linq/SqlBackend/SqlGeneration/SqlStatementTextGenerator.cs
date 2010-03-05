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
    private MethodCallSqlGeneratorRegistry _registry;

    public SqlCommand Build (SqlStatement sqlStatement)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      GenerateSqlGeneratorRegistry();

      var commandBuilder = new SqlCommandBuilder();
      commandBuilder.Append ("SELECT ");
      BuildSelectPart (sqlStatement.SelectProjection, sqlStatement.Count, sqlStatement.Distinct, sqlStatement.TopExpression, commandBuilder);
      commandBuilder.Append (" FROM ");
      BuildFromPart (sqlStatement.FromExpression, commandBuilder);

      return new SqlCommand (commandBuilder.GetCommandText(), commandBuilder.GetCommandParameters());
    }

    protected void BuildSelectPart (Expression expression, bool count, bool distinct, Expression topExpression, SqlCommandBuilder commandBuilder)
    {
      if (((count) && (topExpression != null)) || ((count) && (distinct)))
        throw new ArgumentException ("Wrong argument values. Check values for Count, Distinct and TopExpression.");

      if (count)
        commandBuilder.Append ("COUNT(*)");
      else
      {
        if (distinct)
          commandBuilder.Append ("DISTINCT ");
        else if (topExpression != null)
        {
          commandBuilder.Append ("TOP(");
          SqlGeneratingExpressionVisitor.GenerateSql (topExpression, commandBuilder, _registry);
          commandBuilder.Append (") ");
        }
        SqlGeneratingExpressionVisitor.GenerateSql (expression, commandBuilder, _registry);
      }
    }

    protected void BuildFromPart (SqlTable sqlTable, SqlCommandBuilder commandBuilder)
    {
      SqlTableSourceVisitor.GenerateSql (sqlTable, commandBuilder);
    }

    private void GenerateSqlGeneratorRegistry ()
    {
      _registry = new MethodCallSqlGeneratorRegistry();

      //TODO: Convert methods with all overloads
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
      var removeMethod = typeof (string).GetMethod ("Remove", new Type[] { typeof (int), typeof (int) });
      var startsWithMethod = typeof (string).GetMethod ("StartsWith", new Type[] { typeof (string) });
      var substringMethod = typeof (string).GetMethod ("Substring", new Type[] { typeof (int), typeof (int) });
      var toUpperMethod = typeof (string).GetMethod ("ToUpper", new Type[] { });

      _registry.Register (containsMethod, new MethodCallContains());
      _registry.Register (convertToStringMethod, new MethodCallConvert());
      _registry.Register (convertToBoolMethod, new MethodCallConvert());
      _registry.Register (convertToInt64Method, new MethodCallConvert());
      _registry.Register (convertToDateTimeMethod, new MethodCallConvert());
      _registry.Register (convertToDoubleMethod, new MethodCallConvert());
      _registry.Register (convertToIntMethod, new MethodCallConvert());
      _registry.Register (convertToDecimalMethod, new MethodCallConvert());
      _registry.Register (convertToCharMethod, new MethodCallConvert());
      _registry.Register (convertToByteMethod, new MethodCallConvert());
      _registry.Register (endsWithMethod, new MethodCallEndsWith());
      _registry.Register (lowerMethod, new MethodCallLower());
      _registry.Register (removeMethod, new MethodCallRemove());
      _registry.Register (startsWithMethod, new MethodCallStartsWith());
      _registry.Register (substringMethod, new MethodCallSubstring());
      _registry.Register (toUpperMethod, new MethodCallUpper());
    }
  }
}