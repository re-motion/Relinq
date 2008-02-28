using System.Collections.Generic;
using System.Reflection;
using Rubicon.Collections;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public interface IResolveFieldAccessPolicy
  {
    Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelation (MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers);
  }
}