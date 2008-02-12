using System;
using System.Reflection;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public static class DatabaseInfoUtility
  {
    public static Table GetTableForFromClause (IDatabaseInfo databaseInfo, FromClauseBase fromClause)
    {
      Table? table = databaseInfo.GetTable (fromClause);
      if (table == null)
      {
        string message = string.Format ("The from clause with identifier {0} and query source type {1} does not identify a queryable table.",
            fromClause.Identifier, fromClause.GetQuerySourceType().FullName);
        throw new ArgumentException (message, "fromClause");
      }
      else
        return table.Value;
    }

    public static Column? GetColumn (IDatabaseInfo databaseInfo, Table table, MemberInfo member)
    {
      string columnName = member == null ? "*" : databaseInfo.GetColumnName (member);
      if (columnName == null)
        return null;
      else
        return new Column (table, columnName);
    }
  }
}