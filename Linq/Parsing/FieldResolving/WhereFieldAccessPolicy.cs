using System.Collections.Generic;
using System.Reflection;
using Rubicon.Collections;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public class WhereFieldAccessPolicy : IResolveFieldAccessPolicy
  {
    public Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelation (MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (accessedMember, joinMembers);
    }
  }
}