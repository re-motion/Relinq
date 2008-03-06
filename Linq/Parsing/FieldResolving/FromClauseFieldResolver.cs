using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public class FromClauseFieldResolver
  {
    private readonly IDatabaseInfo _databaseInfo;
    private readonly JoinedTableContext _context;
    private readonly IResolveFieldAccessPolicy _policy;

    public FromClauseFieldResolver (IDatabaseInfo databaseInfo, JoinedTableContext context, IResolveFieldAccessPolicy policy)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("policy", policy);

      _databaseInfo = databaseInfo;
      _context = context;
      _policy = policy;
    }

    public FieldDescriptor ResolveField (FromClauseBase fromClause, Expression partialFieldExpression, Expression fullFieldExpression )
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);
      
      FromClauseFieldResolverVisitor visitor = new FromClauseFieldResolverVisitor ();
      FromClauseFieldResolverVisitor.Result result = visitor.ParseFieldAccess (partialFieldExpression, fullFieldExpression);

      CheckParameterNameAndType (fromClause, result.Parameter);
      return CreateFieldDescriptor (fromClause, result.AccessedMember, result.JoinMembers);
    }

    private void CheckParameterNameAndType (FromClauseBase fromClause, ParameterExpression parameter)
    {
      if (parameter.Name != fromClause.Identifier.Name)
      {
        string message = string.Format ("This from clause can only resolve field accesses for parameters called '{0}', but a parameter "
            + "called '{1}' was given.", fromClause.Identifier.Name, parameter.Name);
        throw new FieldAccessResolveException (message);
      }

      if (parameter.Type != fromClause.Identifier.Type)
      {
        string message = string.Format ("This from clause can only resolve field accesses for parameters of type '{0}', but a parameter "
            + "of type '{1}' was given.", fromClause.Identifier.Type, parameter.Type);
        throw new FieldAccessResolveException (message);
      }
    }

    private FieldDescriptor CreateFieldDescriptor (FromClauseBase fromClause, MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      // Documentation example: sdd.Student_Detail.Student.First
      // joinMembers == "Student_Detail", "Student"

      Table initialTable = fromClause.GetTable(_databaseInfo); // Table for sdd

      var memberInfos = AdjustMemberInfosForRelations (accessedMember, joinMembers);
      MemberInfo accessedMemberForColumn = memberInfos.A;
      IEnumerable<MemberInfo> joinMembersForCalculation = memberInfos.B;

      FieldSourcePathBuilder pathBuilder = new FieldSourcePathBuilder();
      FieldSourcePath fieldData = pathBuilder.BuildFieldSourcePath (_databaseInfo, _context, initialTable, joinMembersForCalculation);
      Column? column = DatabaseInfoUtility.GetColumn (_databaseInfo, fieldData.LastTable, accessedMemberForColumn);
      return new FieldDescriptor (accessedMember, fromClause, fieldData, column);
    }

    private Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelations (MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      if (accessedMember != null && DatabaseInfoUtility.IsRelationMember (_databaseInfo, accessedMember))
        return _policy.AdjustMemberInfosForRelation (accessedMember, joinMembers);
      else
        return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (accessedMember, joinMembers);
    }
 }
}