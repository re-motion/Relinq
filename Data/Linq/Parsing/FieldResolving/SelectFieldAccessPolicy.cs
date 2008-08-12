/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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

    public bool OptimizeRelatedKeyAccess ()
    {
      return false;
    }
  }
}
