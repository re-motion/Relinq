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
    private static readonly InterlockedCache<Tuple<IDatabaseInfo, FromClauseBase>, Table> _tableCache =
        new InterlockedCache<Tuple<IDatabaseInfo, FromClauseBase>, Table>();

    public static Table GetTableForFromClause (IDatabaseInfo databaseInfo, FromClauseBase fromClause)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      return _tableCache.GetOrCreateValue (Tuple.NewTuple (databaseInfo, fromClause), key => GetTableFromDatabaseInfo (databaseInfo, fromClause));
    }

    private static Table GetTableFromDatabaseInfo (IDatabaseInfo databaseInfo, FromClauseBase fromClause)
    {
      string tableName = databaseInfo.GetTableName (fromClause);
      if (tableName == null)
      {
        string message = string.Format ("The from clause with identifier {0} and query source type {1} does not identify a queryable table.",
            fromClause.Identifier, fromClause.GetQueriedEntityType().FullName);
        throw new ArgumentException (message, "fromClause");
      }
      else
        return new Table (tableName, fromClause.Identifier.Name);
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

    public static Tuple<string, string> GetJoinColumnNames (IDatabaseInfo databaseInfo, MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      Tuple<string, string> columns = databaseInfo.GetJoinColumnNames (relationMember);
      if (columns == null)
      {
        string message =
            string.Format ("The member '{0}.{1}' does not identify a relation.", relationMember.DeclaringType.FullName, relationMember.Name);
        throw new InvalidOperationException (message);
      }
      else
        return columns;
    }

    public static bool IsRelationMember (IDatabaseInfo databaseInfo, MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("member", member);

      return databaseInfo.GetRelatedTableName (member) != null;
    }

    public static bool IsVirtualColumn (IDatabaseInfo databaseInfo, MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      return (IsRelationMember (databaseInfo, member)) && (databaseInfo.GetColumnName (member) == null);
    }

    public static Column? GetColumn (IDatabaseInfo databaseInfo, IFromSource fromSource, MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      string columnName = member == null ? "*" : databaseInfo.GetColumnName (member);
      if (columnName == null)
        return null;
      else
        return new Column (fromSource, columnName);
    }
    
    public static MemberInfo GetPrimaryKeyMember (IDatabaseInfo databaseInfo, Type entityType)
    {
      MemberInfo primaryKeyMember = databaseInfo.GetPrimaryKeyMember (entityType);
      if (primaryKeyMember == null)
      {
        var message = string.Format ("The primary key member of type '{0}' cannot be determined because it is no entity type.", entityType.FullName);
        throw new InvalidOperationException (message);
      }
      else
        return primaryKeyMember;
    }
  }
}