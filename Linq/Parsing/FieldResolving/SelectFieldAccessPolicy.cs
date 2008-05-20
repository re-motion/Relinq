using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Collections;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  public class SelectFieldAccessPolicy : IResolveFieldAccessPolicy
  {
    public Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForAccessedIdentifier (ParameterExpression accessedIdentifier)
    {
      return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (null, new MemberInfo[0]);
    }

    public Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelation (MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
        List<MemberInfo> newJoinMembers = new List<MemberInfo> (joinMembers);
        newJoinMembers.Add (accessedMember);
        return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (null, newJoinMembers); // select full table if relation member is accessed
    }
  }
}