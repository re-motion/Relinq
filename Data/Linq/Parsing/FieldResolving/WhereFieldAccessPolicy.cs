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
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  public class WhereFieldAccessPolicy : IResolveFieldAccessPolicy
  {
    private readonly IDatabaseInfo _databaseInfo;

    public WhereFieldAccessPolicy (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      _databaseInfo = databaseInfo;
    }

    public Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForAccessedIdentifier (ParameterExpression accessedIdentifier)
    {
      ArgumentUtility.CheckNotNull ("accessedIdentifier", accessedIdentifier);
      return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (_databaseInfo.GetPrimaryKeyMember (accessedIdentifier.Type), 
          new MemberInfo[0]);
    }

    public Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelation (MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      ArgumentUtility.CheckNotNull ("accessedMember", accessedMember);
      ArgumentUtility.CheckNotNull ("joinMembers", joinMembers);
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
