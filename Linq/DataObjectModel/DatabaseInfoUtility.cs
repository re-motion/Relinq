using System;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public static class DatabaseInfoUtility
  {
    public static Table GetTableForFromClause (IDatabaseInfo databaseInfo, FromClauseBase fromClause)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      Table table = databaseInfo.GetTable (fromClause);
      if (table == null)
      {
        string message = string.Format ("The from clause with identifier {0} and query source type {1} does not identify a queryable table.",
            fromClause.Identifier, fromClause.GetQuerySourceType().FullName);
        throw new ArgumentException (message, "fromClause");
      }
      else
        return table;
    }

    public static Column? GetColumn (IDatabaseInfo databaseInfo, Table table, MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      string columnName = member == null ? "*" : databaseInfo.GetColumnName (member);
      if (columnName == null)
        return null;
      else
        return new Column (table, columnName);
    }

    public static Table GetRelatedTable (IDatabaseInfo databaseInfo, MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      string tableName = databaseInfo.GetRelatedTableName (relationMember);
      if (tableName == null)
      {
        string message =
            string.Format ("The member '{0}.{1}' does not identify a relation.", relationMember.DeclaringType.FullName, relationMember.Name);
        throw new InvalidOperationException (message);
      }
      else
        return new Table (tableName, null);
    }

    public static Tuple<string, string> GetJoinColumns (IDatabaseInfo databaseInfo, MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      Tuple<string, string> columns = databaseInfo.GetJoinColumns (relationMember);
      if (columns == null)
      {
        string message =
            string.Format ("The member '{0}.{1}' does not identify a relation.", relationMember.DeclaringType.FullName, relationMember.Name);
        throw new InvalidOperationException (message);
      }
      else
        return columns;
    }
  }
}