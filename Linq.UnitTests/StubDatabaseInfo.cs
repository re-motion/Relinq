using System;
using System.Linq;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests
{
  public class StubDatabaseInfo : IDatabaseInfo
  {
    public static readonly StubDatabaseInfo Instance = new StubDatabaseInfo();

    private StubDatabaseInfo ()
    {
    }

    public Table? GetTable (FromClauseBase fromClause)
    {
      Type querySourceType = fromClause.GetQuerySourceType();
      if (typeof (IQueryable<Student>).IsAssignableFrom (querySourceType))
        return new Table("sourceTable", fromClause.Identifier.Name);
      else if (typeof (IQueryable<Student_Detail>).IsAssignableFrom (querySourceType))
        return new Table("detailTable", fromClause.Identifier.Name);
      else if (typeof (IQueryable<Student_Detail_Detail>).IsAssignableFrom (querySourceType))
        return new Table ("detailDetailTable", fromClause.Identifier.Name);
      else
        return null;
    }

    public string GetRelatedTableName (MemberInfo relationMember)
    {
      if (relationMember == typeof (Student_Detail).GetProperty ("Student"))
        return "sourceTable";
      else if (relationMember == typeof (Student_Detail_Detail).GetProperty ("Student_Detail"))
        return "detailTable";
      else
        return null;
    }

    public string GetColumnName (MemberInfo member)
    {
      if (member.Name == "NonDBProperty" || member.Name == "NonDBBoolProperty")
        return null;
      else
        return member.Name + "Column";
    }

    public Tuple<string, string> GetJoinColumns (MemberInfo relationMember)
    {
      if (relationMember == typeof (Student_Detail).GetProperty ("Student"))
        return Tuple.NewTuple ("Student_Detail_PK", "Student_FK");
      else if (relationMember == typeof (Student_Detail_Detail).GetProperty ("Student_Detail"))
        return Tuple.NewTuple ("Student_Detail_Detail_PK", "Student_Detail_FK");
      else
        return null;
    }
  }
}