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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Collections;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  public class FieldResolver
  {
    public IDatabaseInfo DatabaseInfo { get; private set; }
    private readonly IResolveFieldAccessPolicy _policy;

    public FieldResolver (IDatabaseInfo databaseInfo, IResolveFieldAccessPolicy policy)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("policy", policy);

      DatabaseInfo = databaseInfo;
      _policy = policy;
    }

    public FieldDescriptor ResolveField (Expression fieldAccessExpression, JoinedTableContext joinedTableContext)
    {
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);
      
      var result = FieldResolverVisitor.ParseFieldAccess (DatabaseInfo, fieldAccessExpression, _policy.OptimizeRelatedKeyAccess());
      var clause = result.QuerySourceReferenceExpression.ReferencedClause;
      return CreateFieldDescriptor (joinedTableContext.GetColumnSource (clause), result.QuerySourceReferenceExpression, result.AccessedMember, result.JoinMembers, joinedTableContext);
    }

    private FieldDescriptor CreateFieldDescriptor (IColumnSource firstSource, QuerySourceReferenceExpression referenceExpression, MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers, JoinedTableContext joinedTableContext)
    {
      // Documentation example: sdd.Student_Detail.Student.First
      // joinMembers == "Student_Detail", "Student"

      var memberInfos = AdjustMemberInfos (referenceExpression, accessedMember, joinMembers);
      MemberInfo accessedMemberForColumn = memberInfos.A;
      IEnumerable<MemberInfo> joinMembersForCalculation = memberInfos.B;

      var pathBuilder = new FieldSourcePathBuilder();
      FieldSourcePath fieldData = pathBuilder.BuildFieldSourcePath (DatabaseInfo, joinedTableContext, firstSource, joinMembersForCalculation);

      Column? column = DatabaseInfoUtility.GetColumn (DatabaseInfo, fieldData.LastSource, accessedMemberForColumn);
      return new FieldDescriptor (accessedMember, fieldData, column);
    }
    
    private Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfos (QuerySourceReferenceExpression referenceExpression, MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      if (accessedMember == null)
      {
        Assertion.IsTrue (joinMembers.Count() == 0);
        return _policy.AdjustMemberInfosForDirectAccessOfQuerySource (referenceExpression);
      }
      else if (DatabaseInfoUtility.IsRelationMember (DatabaseInfo, accessedMember))
        return _policy.AdjustMemberInfosForRelation (accessedMember, joinMembers);
      else
        return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (accessedMember, joinMembers);
    }
 }
}
