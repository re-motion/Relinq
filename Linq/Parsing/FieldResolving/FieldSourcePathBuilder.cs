using System;
using System.Collections.Generic;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public class FieldSourcePathBuilder
  {
    public Tuple<IFieldSourcePath, Table> BuildFieldSourcePath (IDatabaseInfo databaseInfo, JoinedTableContext context, Table initialTable, IEnumerable<MemberInfo> joinMembers)
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

          fieldSourcePath = new JoinTree (leftTable, fieldSourcePath, leftColumn, rightColumn);
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