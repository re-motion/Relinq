using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Rubicon.Collections;
using Rubicon.Data.Linq.Visitor;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq
{
  public class SqlGenerator
  {
    private readonly QueryExpression _query;

    public SqlGenerator (QueryExpression query)
    {
      ArgumentUtility.CheckNotNull ("query", query);
      _query = query;
    }

    public IDbCommand GetCommand (IDatabaseInfo databaseInfo, IDbConnection connection)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("connection", connection);

      SqlGeneratorVisitor visitor  = new SqlGeneratorVisitor (databaseInfo);
      _query.Accept (visitor);
      StringBuilder sb = new StringBuilder();

      sb.Append ("SELECT ");

      if (visitor.Columns.Count == 0)
        sb.Append ("* ");
      else
      {
        IEnumerable<string> columnEntries = JoinTupleItems (visitor.Columns, ".");
        sb.Append (SeparatedStringBuilder.Build (", ", columnEntries)).Append (" ");
      }

      sb.Append ("FROM ");

      IEnumerable<string> tableEntries = JoinTupleItems (visitor.Tables, " ");
      sb.Append (SeparatedStringBuilder.Build (", ", tableEntries));

      IDbCommand command = connection.CreateCommand();
      command.CommandText = sb.ToString();
      command.CommandType = CommandType.Text;
      return command;
    }

    private IEnumerable<string> JoinTupleItems (IEnumerable<Tuple<string, string>> tuples, string joinString)
    {
      foreach (Tuple<string, string> tuple in tuples)
        yield return WrapSqlIdentifier (tuple.A) + joinString + WrapSqlIdentifier (tuple.B);
    }

    private string WrapSqlIdentifier (string identifier)
    {
      if (identifier != "*")
        return "[" + identifier + "]";
      else
        return "*";
    }
  }
}