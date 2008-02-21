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
    private readonly FromClauseBase _fromClause;

    public FromClauseFieldResolver (FromClauseBase fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      _fromClause = fromClause;
    }

    public FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, JoinedTableContext context, Expression partialFieldExpression, Expression fullFieldExpression)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);
      
      FromClauseFieldResolverVisitor visitor = new FromClauseFieldResolverVisitor ();
      FromClauseFieldResolverVisitor.Result result = visitor.ParseFieldAccess (partialFieldExpression, fullFieldExpression);

      CheckParameterNameAndType (result.Parameter);
      return CreateFieldDescriptor (databaseInfo, context, result.AccessedMember, result.JoinMembers);
    }

    private void CheckParameterNameAndType (ParameterExpression parameter)
    {
      if (parameter.Name != _fromClause.Identifier.Name)
      {
        string message = string.Format ("This from clause can only resolve field accesses for parameters called '{0}', but a parameter "
            + "called '{1}' was given.", _fromClause.Identifier.Name, parameter.Name);
        throw new FieldAccessResolveException (message);
      }

      if (parameter.Type != _fromClause.Identifier.Type)
      {
        string message = string.Format ("This from clause can only resolve field accesses for parameters of type '{0}', but a parameter "
            + "of type '{1}' was given.", _fromClause.Identifier.Type, parameter.Type);
        throw new FieldAccessResolveException (message);
      }
    }

    private FieldDescriptor CreateFieldDescriptor (IDatabaseInfo databaseInfo, JoinedTableContext context, MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      // Documentation example: sdd.Student_Detail.Student.First
      // joinMembers == "Student_Detail", "Student"

      Table initialTable = _fromClause.GetTable(databaseInfo); // Table for sdd

      var memberInfos = AdjustMemberInfosForRelations (databaseInfo, accessedMember, joinMembers);
      MemberInfo accessedMemberForColumn = memberInfos.A;
      IEnumerable<MemberInfo> joinMembersForCalculation = memberInfos.B;

      FieldSourcePathBuilder pathBuilder = new FieldSourcePathBuilder();
      FieldSourcePath fieldData = pathBuilder.BuildFieldSourcePath2 (databaseInfo, context, initialTable, joinMembersForCalculation);
      Column? column = DatabaseInfoUtility.GetColumn (databaseInfo, fieldData.LastTable, accessedMemberForColumn);
      return new FieldDescriptor (accessedMember, _fromClause, fieldData, column);
    }

    private Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelations (
        IDatabaseInfo databaseInfo, MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      if (accessedMember != null && DatabaseInfoUtility.IsRelationMember (databaseInfo, accessedMember))
      {
        List<MemberInfo> newJoinMembers = new List<MemberInfo> (joinMembers);
        newJoinMembers.Add (accessedMember);
        return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (null, newJoinMembers); // select full table if relation member is accessed
      }
      else
        return new Tuple<MemberInfo, IEnumerable<MemberInfo>> (accessedMember, joinMembers);
    }
  }
}