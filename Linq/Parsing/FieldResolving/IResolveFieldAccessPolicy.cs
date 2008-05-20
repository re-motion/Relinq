using System.Collections.Generic;
using System.Reflection;
using Remotion.Collections;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  public interface IResolveFieldAccessPolicy
  {
    Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForAccessedIdentifier (ParameterExpression accessedIdentifier);
    Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelation (MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers);
  }
}