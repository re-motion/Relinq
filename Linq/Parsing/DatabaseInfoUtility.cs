using System;
using System.Reflection;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.Parsing
{
  public static class DatabaseInfoUtility
  {
    public static Table GetTableForFromClause (IDatabaseInfo databaseInfo, FromClauseBase fromClause)
    {
      string tableName = databaseInfo.GetTableName (fromClause.GetQuerySourceType());
      if (tableName == null)
      {
        string message = string.Format ("The from clause with identifier {0} and query source type {1} does not identify a queryable table.",
            fromClause.Identifier, fromClause.GetQuerySourceType().FullName);
        throw new ArgumentException (message, "fromClause");
      }
      return new Table(tableName, fromClause.Identifier.Name);
    }

    public static Column GetColumn (IDatabaseInfo databaseInfo, Table table, MemberInfo member)
    {
      string columnName = member == null ? "*" : databaseInfo.GetColumnName (member);
      if (columnName == null)
      {
        string message = string.Format ("The member {0}.{1} does not identify a queryable column in table {2}.",
            member.DeclaringType.FullName, member.Name, table.Name);
        throw new ArgumentException (message, "member");
      }
      return new Column (table, columnName);
    }
  }
}