using System;
using System.Reflection;
using Remotion.Data.Linq.Clauses;
using Remotion.Collections;

namespace Remotion.Data.Linq
{
  public interface IDatabaseInfo
  {
    string GetTableName (FromClauseBase fromClause);
    string GetRelatedTableName (MemberInfo relationMember);
    string GetColumnName (MemberInfo member);
    Tuple<string, string> GetJoinColumnNames (MemberInfo relationMember);
    object ProcessWhereParameter (object parameter);
    MemberInfo GetPrimaryKeyMember (Type entityType);
    bool IsTableType (Type type);
  }
}