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
          Table relatedTable = context.GetJoinedTable (databaseInfo, pathSoFar, member);
          Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumnNames (databaseInfo, member);

          Column leftColumn = new Column (lastTable, joinColumns.A);
          Column rightColumn = new Column (relatedTable, joinColumns.B);

          joins.Add (new SingleJoin (leftColumn, rightColumn));
          lastTable = relatedTable;
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