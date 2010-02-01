// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Reflection;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq.UnitTests.TestDomain
{
  public class StubDatabaseInfo : IDatabaseInfo
  {
    public static readonly StubDatabaseInfo Instance = new StubDatabaseInfo();

    private StubDatabaseInfo ()
    {
    }

    public Table GetTableForFromClause (FromClauseBase fromClause, string alias)
    {
      var tableName = GetTableName (fromClause);
      if (tableName == null)
        throw new UnmappedItemException (string.Format ("'{0} {1}' is not a table.", fromClause.ItemType, fromClause.ItemName));
      return new Table (tableName, alias);
    }

    public bool IsRelationMember (MemberInfo member)
    {
      var tableName = GetRelatedTableName (member);
      return tableName != null;
    }

    public Table GetTableForRelation (MemberInfo relationMember, string alias)
    {
      var tableName = GetRelatedTableName (relationMember);
      if (tableName == null)
        throw new UnmappedItemException (string.Format ("'{0}.{1}' is not a relation member.", relationMember.DeclaringType, relationMember.Name));

      return new Table (tableName, alias);
    }

    public bool HasColumn (MemberInfo member)
    {
      return GetColumnName (member) != null;
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
      else if (member == typeof (IndustrialSector).GetProperty ("Students"))
        return null;
      else
        return member.Name + "Column";
    }

    public JoinColumnNames? GetJoinColumnNames (MemberInfo relationMember)
    {
      if (relationMember == typeof (Student_Detail).GetProperty ("Student"))
        return new JoinColumnNames("Student_Detail_PK", "Student_Detail_to_Student_FK");
      else if (relationMember == typeof (Student_Detail_Detail).GetProperty ("Student_Detail"))
        return new JoinColumnNames("Student_Detail_Detail_PK", "Student_Detail_Detail_to_Student_Detail_FK");
      else if (relationMember == typeof (Student_Detail_Detail).GetProperty ("IndustrialSector"))
        return new JoinColumnNames("Student_Detail_Detail_PK", "Student_Detail_Detail_to_IndustrialSector_FK");
      else if (relationMember == typeof (IndustrialSector).GetProperty ("Student_Detail"))
        return new JoinColumnNames ("IndustrialSector_PK", "Student_Detail_to_IndustrialSector_FK");
      else if (relationMember == typeof (Student_Detail).GetProperty ("IndustrialSector"))
        return new JoinColumnNames("Student_Detail_to_IndustrialSector_FK", "IndustrialSector_PK");
      else if (relationMember == typeof (Student).GetProperty ("OtherStudent"))
        return new JoinColumnNames ("Student_to_OtherStudent_FK", "Student_PK");
      else if (relationMember == typeof (IndustrialSector).GetProperty ("Students"))
        return new JoinColumnNames ("Industrial_PK", "Student_to_IndustrialSector_FK");
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

    private string GetTableName (FromClauseBase fromClause)
    {
      switch (fromClause.ItemType.Name)
      {
        case "Student":
          return "studentTable";
        case "Student_Detail":
          return "detailTable";
        case "Student_Detail_Detail":
          return "detailDetailTable";
        case "IndustrialSector":
          return "industrialTable";
        default:
          return null;
      }
    }

    private string GetRelatedTableName (MemberInfo relationMember)
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
      else if (relationMember == typeof (Student).GetProperty ("OtherStudent"))
        return "studentTable";
      else if (relationMember == typeof (IndustrialSector).GetProperty ("Students"))
        return "studentTable";
      else
        return null;
    }

  }
}
