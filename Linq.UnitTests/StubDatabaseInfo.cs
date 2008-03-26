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

    public string GetTableName (FromClauseBase fromClause)
    {
      Type querySourceType = fromClause.GetQueriedEntityType();
      if (typeof (IQueryable<Student>).IsAssignableFrom (querySourceType))
        return "studentTable";
      else if (typeof (IQueryable<Student_Detail>).IsAssignableFrom (querySourceType))
        return "detailTable";
      else if (typeof (IQueryable<Student_Detail_Detail>).IsAssignableFrom (querySourceType))
        return "detailDetailTable";
      else if (typeof(IQueryable<IndustrialSector>).IsAssignableFrom(querySourceType))
        return "industrialTable";
      else
        return null;
    }

    public string GetRelatedTableName (MemberInfo relationMember)
    {
      if (relationMember == typeof (Student_Detail).GetProperty ("Student"))
        return "studentTable";
      else if (relationMember == typeof (Student_Detail_Detail).GetProperty ("Student_Detail"))
        return "detailTable";
      else if (relationMember == typeof (Student_Detail_Detail).GetProperty ("IndustrialSector"))
        return "industrialTable";
      else if (relationMember == typeof (Student_Detail).GetProperty ("IndustrialSector"))
        return "industrialTable";
      else if (relationMember == typeof (IndustrialSector).GetProperty ("Student_Detail"))
        return "detailTable";
      else
        return null;
    }

    public string GetColumnName (MemberInfo member)
    {
      if (member.Name == "NonDBProperty" || member.Name == "NonDBBoolProperty")
        return null;
      else if (member == typeof (Student_Detail).GetProperty ("Student"))
        return null;
      else if (member == typeof (Student_Detail_Detail).GetProperty ("Student_Detail"))
        return null;
      else if (member == typeof (Student_Detail_Detail).GetProperty ("IndustrialSector"))
        return null;
      else if (member == typeof (IndustrialSector).GetProperty ("Student_Detail"))
        return null;
      else if (member == typeof (Student_Detail).GetProperty ("IndustrialSector"))
        return "Student_Detail_to_IndustrialSector_FK";
      else
        return member.Name + "Column";
    }

    public Tuple<string, string> GetJoinColumnNames (MemberInfo relationMember)
    {
      if (relationMember == typeof (Student_Detail).GetProperty ("Student"))
        return Tuple.NewTuple ("Student_Detail_PK", "Student_Detail_to_Student_FK");
      else if (relationMember == typeof (Student_Detail_Detail).GetProperty ("Student_Detail"))
        return Tuple.NewTuple ("Student_Detail_Detail_PK", "Student_Detail_Detail_to_Student_Detail_FK");
      else if (relationMember == typeof (Student_Detail_Detail).GetProperty ("IndustrialSector"))
        return Tuple.NewTuple ("Student_Detail_Detail_PK", "Student_Detail_Detail_to_IndustrialSector_FK");
      else if (relationMember == typeof (IndustrialSector).GetProperty ("Student_Detail"))
        return Tuple.NewTuple ("IndustrialSector_PK", "Student_Detail_to_IndustrialSector_FK");
      else if (relationMember == typeof (Student_Detail).GetProperty ("IndustrialSector"))
        return Tuple.NewTuple ("Student_Detail_to_IndustrialSector_FK", "IndustrialSector_PK");
      else
        return null;
    }

    public object ProcessWhereParameter (object parameter)
    {
      Student student = parameter as Student;
      if (student != null)
        return student.ID;
      return parameter;
    }

    public MemberInfo GetPrimaryKeyMember (Type entityType)
    {
      if (entityType == typeof (Student_Detail))
        return typeof (Student_Detail).GetProperty ("ID");
      else if (entityType == typeof (Student))
        return typeof (Student).GetProperty ("ID");
      else if (entityType == typeof (IndustrialSector))
        return typeof (IndustrialSector).GetProperty ("ID");
      else
        return null;
    }
  }
}