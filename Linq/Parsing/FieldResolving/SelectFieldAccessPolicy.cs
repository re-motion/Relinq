using System.Collections.Generic;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public class SelectFieldAccessPolicy : IResolveFieldAccessPolicy
  {
    public Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelation (MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
        List<MemberInfo> newJoinMembers = new List<MemberInfo> (joinMembers);
        newJoinMembers.Add (accessedMember);
        return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (null, newJoinMembers); // select full table if relation member is accessed
    }
  }
}