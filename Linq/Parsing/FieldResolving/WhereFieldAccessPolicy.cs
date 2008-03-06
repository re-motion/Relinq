using System.Collections.Generic;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;
using System.Linq;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public class WhereFieldAccessPolicy : IResolveFieldAccessPolicy
  {
    private readonly IDatabaseInfo _databaseInfo;

    public WhereFieldAccessPolicy (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      _databaseInfo = databaseInfo;
    }

    public Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelation (MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      if (DatabaseInfoUtility.IsVirtualColumn (_databaseInfo, accessedMember))
      {
        MemberInfo primaryKeyMember = DatabaseInfoUtility.GetPrimaryKeyMember (_databaseInfo, ReflectionUtility.GetFieldOrPropertyType (accessedMember));
        return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (primaryKeyMember, joinMembers.Concat (new[] {accessedMember}));
      }
      else
        return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (accessedMember, joinMembers);
    }
  }
}