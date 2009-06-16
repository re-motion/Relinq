// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
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

    public Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForDirectAccessOfQuerySource (ParameterExpression accessedIdentifier)
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

    public bool OptimizeRelatedKeyAccess ()
    {
      return true;
    }
  }
}
