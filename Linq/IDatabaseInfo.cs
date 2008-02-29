using System;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.Linq
{
  public interface IDatabaseInfo
  {
    string GetTableName (FromClauseBase fromClause);
    string GetRelatedTableName (MemberInfo relationMember);
    string GetColumnName (MemberInfo member);
    Tuple<string, string> GetJoinColumnNames (MemberInfo relationMember);
    object ProcessWhereParameter (object parameter);
  }
}