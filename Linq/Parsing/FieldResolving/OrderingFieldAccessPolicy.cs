using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public class OrderingFieldAccessPolicy : IResolveFieldAccessPolicy
  {
    public Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForAccessedIdentifier (ParameterExpression accessedIdentifier)
    {
      return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (null, new MemberInfo[0]);
    }

    public Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelation (MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      string message = string.Format ("Ordering by '{0}.{1}' is not supported because it is a relation member.", accessedMember.DeclaringType.FullName,
          accessedMember.Name);
      throw new NotSupportedException (message);
    }
  }
}