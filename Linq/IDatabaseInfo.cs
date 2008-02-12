using System;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.Linq
{
  public interface IDatabaseInfo
  {
    Table? GetTable (FromClauseBase fromClause);
    string GetRelatedTableName (MemberInfo relationMember);
    string GetColumnName (MemberInfo member);
    Tuple<string, string> GetJoinColumns (MemberInfo relationMember);
  }
}