using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public class ClauseFieldResolver
  {
    public IDatabaseInfo DatabaseInfo { get; private set; }
    public JoinedTableContext Context { get; private set; }

    private readonly IResolveFieldAccessPolicy _policy;

    public ClauseFieldResolver (IDatabaseInfo databaseInfo, JoinedTableContext context, IResolveFieldAccessPolicy policy)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("policy", policy);

      DatabaseInfo = databaseInfo;
      Context = context;
      _policy = policy;
    }

    public FieldDescriptor ResolveField (IColumnSource columnSource, ParameterExpression clauseIdentifier, Expression partialFieldExpression, Expression fullFieldExpression)
    {
      ArgumentUtility.CheckNotNull ("fromSource", columnSource);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);
      
      var visitor = new ClauseFieldResolverVisitor ();
      ClauseFieldResolverVisitor.Result result = visitor.ParseFieldAccess (partialFieldExpression, fullFieldExpression);

      CheckParameterNameAndType (clauseIdentifier, result.Parameter);
      return CreateFieldDescriptor (columnSource, clauseIdentifier, result.AccessedMember, result.JoinMembers);
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

    private FieldDescriptor CreateFieldDescriptor (IColumnSource firstSource, ParameterExpression accessedIdentifier, MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers)
    {
      // Documentation example: sdd.Student_Detail.Student.First
      // joinMembers == "Student_Detail", "Student"

      var memberInfos = AdjustMemberInfos (accessedIdentifier, accessedMember, joinMembers);
      MemberInfo accessedMemberForColumn = memberInfos.A;
      IEnumerable<MemberInfo> joinMembersForCalculation = memberInfos.B;

      FieldSourcePathBuilder pathBuilder = new FieldSourcePathBuilder();
      FieldSourcePath fieldData = pathBuilder.BuildFieldSourcePath (DatabaseInfo, Context, firstSource, joinMembersForCalculation);

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