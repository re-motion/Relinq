using System.Reflection;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq
{
#warning TODO: error handling
  public static class DatabaseInfoUtility
  {
    public static Table GetTableForFromClause (IDatabaseInfo databaseInfo, FromClauseBase fromClause)
    {
      return new Table(databaseInfo.GetTableName (fromClause.GetQuerySourceType()), fromClause.Identifier.Name);
    }

    public static Column GetColumn (IDatabaseInfo databaseInfo, Table table, MemberInfo member)
    {
      return new Column (table, member == null ? "*" : databaseInfo.GetColumnName (member));
    }
  }
}