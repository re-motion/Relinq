using System.Collections.Generic;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using System.Linq.Expressions;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public interface IResolveFieldAccessPolicy
  {
    Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForAccessedIdentifier (ParameterExpression accessedIdentifier);
    Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelation (MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers);
  }
}