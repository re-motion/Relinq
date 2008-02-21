using System;
using System.Collections.Generic;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public class FieldSourcePathBuilder
  {
    public FieldSourcePath BuildFieldSourcePath2 (IDatabaseInfo databaseInfo, JoinedTableContext context, Table initialTable, IEnumerable<MemberInfo> joinMembers)
    {
      List<SingleJoin> joins = new List<SingleJoin>();

      Table lastTable = initialTable;
      foreach (MemberInfo member in joinMembers)
      {
        FieldSourcePath pathSoFar = new FieldSourcePath (initialTable, joins);
        try
        {
          Table leftTable = context.GetJoinedTable (databaseInfo, pathSoFar, member);
          Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumnNames (databaseInfo, member);

          // joinColumns holds the columns in the order defined by the member: for "sdd.Student_Detail" it holds sdd.PK/Student_Detail.FK
          // we build the trees in opposite order, so we use the first tuple value as the right column, the second value as the left column
          Column leftColumn = new Column (leftTable, joinColumns.B);
          Column rightColumn = new Column (lastTable, joinColumns.A);

          joins.Add (new SingleJoin(leftColumn, rightColumn));
          lastTable = leftTable;
        }
        catch (Exception ex)
        {
          throw new FieldAccessResolveException (ex.Message, ex);
        }
      }

      return new FieldSourcePath(initialTable, joins);
    }
  }
}