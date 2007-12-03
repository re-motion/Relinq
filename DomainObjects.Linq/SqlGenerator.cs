using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Rubicon.Collections;
using Rubicon.Data.DomainObjects.Linq.QueryProviderImplementation;
using Rubicon.Data.DomainObjects.Linq.Visitor;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
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
        throw new NotImplementedException();

      sb.Append ("FROM ");

      IEnumerable<string> tableEntries = JoinTableEntries (visitor.Tables);
      sb.Append (SeparatedStringBuilder.Build (", ", tableEntries));

      IDbCommand command = connection.CreateCommand();
      command.CommandText = sb.ToString();
      command.CommandType = CommandType.Text;
      return command;
    }

    private IEnumerable<string> JoinTableEntries (IEnumerable<Tuple<string, string>> tableEntries)
    {
      foreach (Tuple<string, string> tableEntry in tableEntries)
        yield return tableEntry.A + " " + tableEntry.B;
    }
  }
}