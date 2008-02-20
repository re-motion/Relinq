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

      Table initialTable = DatabaseInfoUtility.GetTableForFromClause (databaseInfo, _fromClause); // Table for sdd

      var memberInfos = AdjustMemberInfosForRelations (databaseInfo, accessedMember, joinMembers);
      MemberInfo accessedMemberForColumn = memberInfos.A;
      IEnumerable<MemberInfo> joinMembersForCalculation = memberInfos.B;
      
      Tuple<IFieldSourcePath, Table> fieldData = CalculateJoinData (databaseInfo, context, initialTable, joinMembersForCalculation);
      Column? column = DatabaseInfoUtility.GetColumn (databaseInfo, fieldData.B, accessedMemberForColumn);
      return new FieldDescriptor (accessedMember, _fromClause, fieldData.A, column);
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

    private Tuple<IFieldSourcePath, Table> CalculateJoinData (IDatabaseInfo databaseInfo, JoinedTableContext context, Table initialTable, IEnumerable<MemberInfo> joinMembers)
    {
      // Documentation example: sdd.Student_Detail.Student.First
      // First create a join for sdd and Student_Detail (identified by the "Student_Detail" member) and use initial table (sdd) as the right side
      // Then create the join for Student_Detail and Student (identified by the "Student" member) and use the first join as the right side
      //      second join
      //    /            \
      // Student       first join
      //              /          \
      //      Student_Detail    sdd

      Table lastTable = initialTable;
      IFieldSourcePath fieldSourcePath = lastTable;
      foreach (MemberInfo member in joinMembers)
      {
        try
        {
          Table leftTable = context.GetJoinedTable (databaseInfo, fieldSourcePath, member); //DatabaseInfoUtility.GetRelatedTable (databaseInfo, member);
          Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumns (databaseInfo, member);

          // joinColumns holds the columns in the order defined by the member: for "sdd.Student_Detail" it holds sdd.PK/Student_Detail.FK
          // we build the trees in opposite order, so we use the first tuple value as the right column, the second value as the left column
          Column leftColumn = new Column (leftTable, joinColumns.B);
          Column rightColumn = new Column (lastTable, joinColumns.A);

          fieldSourcePath = new Join (leftTable, fieldSourcePath, leftColumn, rightColumn);
          lastTable = leftTable;
        }
        catch (Exception ex)
        {
          throw new FieldAccessResolveException (ex.Message, ex);
        }
      }

      return Tuple.NewTuple (fieldSourcePath, lastTable);
    }
  }
}