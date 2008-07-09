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
