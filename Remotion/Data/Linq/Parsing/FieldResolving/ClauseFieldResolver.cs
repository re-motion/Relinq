// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  public class ClauseFieldResolver
  {
    public IDatabaseInfo DatabaseInfo { get; private set; }
    private readonly IResolveFieldAccessPolicy _policy;

    public ClauseFieldResolver (IDatabaseInfo databaseInfo, IResolveFieldAccessPolicy policy)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("policy", policy);

      DatabaseInfo = databaseInfo;
      _policy = policy;
    }

    public FieldDescriptor ResolveField (IColumnSource columnSource, ParameterExpression clauseIdentifier, Expression partialFieldExpression, Expression fullFieldExpression, JoinedTableContext joinedTableContext)
    {
      ArgumentUtility.CheckNotNull ("fromSource", columnSource);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);
      
      var visitor = new ClauseFieldResolverVisitor (DatabaseInfo);
      ClauseFieldResolverVisitor.Result result = visitor.ParseFieldAccess (partialFieldExpression, fullFieldExpression, _policy.OptimizeRelatedKeyAccess());

      CheckParameterNameAndType (clauseIdentifier, result.Parameter);
      return CreateFieldDescriptor (columnSource, clauseIdentifier, result.AccessedMember, result.JoinMembers, joinedTableContext);
    }

    private void CheckParameterNameAndType (ParameterExpression clauseIdentifier, ParameterExpression parameter)
    {
      if (parameter.Name != clauseIdentifier.Name)
      {
        string message = string.Format ("This clause can only resolve field accesses for parameters called '{0}', but a parameter "
            + "called '{1}' was given.", clauseIdentifier.Name, parameter.Name);
        throw new FieldAccessResolveException (message);
      }

      if (parameter.Type != clauseIdentifier.Type)
      {
        string message = string.Format ("This clause can only resolve field accesses for parameters of type '{0}', but a parameter "
            + "of type '{1}' was given.", clauseIdentifier.Type, parameter.Type);
        throw new FieldAccessResolveException (message);
      }
    }

    private FieldDescriptor CreateFieldDescriptor (IColumnSource firstSource, ParameterExpression accessedIdentifier, MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers, JoinedTableContext joinedTableContext)
    {
      // Documentation example: sdd.Student_Detail.Student.First
      // joinMembers == "Student_Detail", "Student"

      var memberInfos = AdjustMemberInfos (accessedIdentifier, accessedMember, joinMembers);
      MemberInfo accessedMemberForColumn = memberInfos.A;
      IEnumerable<MemberInfo> joinMembersForCalculation = memberInfos.B;

      FieldSourcePathBuilder pathBuilder = new FieldSourcePathBuilder();
      FieldSourcePath fieldData = pathBuilder.BuildFieldSourcePath (DatabaseInfo, joinedTableContext, firstSource, joinMembersForCalculation);

      Column? column = DatabaseInfoUtility.GetColumn (DatabaseInfo, fieldData.LastSource, accessedMemberForColumn);
      return new FieldDescriptor (accessedMember, fieldData, column);
    }
    
    private Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfos (ParameterExpression accessedIdentifier, MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      if (accessedMember == null)
      {
        Assertion.IsTrue (joinMembers.Count() == 0);
        return _policy.AdjustMemberInfosForAccessedIdentifier (accessedIdentifier);
      }
      else if (DatabaseInfoUtility.IsRelationMember (DatabaseInfo, accessedMember))
        return _policy.AdjustMemberInfosForRelation (accessedMember, joinMembers);
      else
        return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (accessedMember, joinMembers);
    }
 }
}
