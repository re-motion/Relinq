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

    public bool HasAssociatedColumn (MemberInfo member)
    {
      return GetColumnName (member) != null;
    }

    public Column GetColumnForMember (IColumnSource columnSource, MemberInfo member)
    {
      var columnName = GetColumnName (member);
      if (columnName == null)
      {
        var message = string.Format ("The member '{0}.{1}' does not identify a queryable column.", member.DeclaringType, member.Name);
        throw new UnmappedItemException (message);
      }
      else
      {
        return new Column (columnSource, columnName);
      }
    }

    public SingleJoin GetJoinForMember (MemberInfo relationMember, IColumnSource leftSource, IColumnSource rightSource)
    {
      if (relationMember == typeof (Kitchen).GetProperty ("Cook"))
        return new SingleJoin (new Column (leftSource, "Kitchen_PK"), new Column (rightSource, "Kitchen_to_Cook_FK"));
      else if (relationMember == typeof (Company).GetProperty ("MainKitchen"))
        return new SingleJoin (new Column (leftSource, "Company_PK"), new Column (rightSource, "Company_to_Kitchen_FK"));
      else if (relationMember == typeof (Company).GetProperty ("Restaurant"))
        return new SingleJoin (new Column (leftSource, "Company_PK"), new Column (rightSource, "Company_to_Restaurant_FK"));
      else if (relationMember == typeof (Restaurant).GetProperty ("SubKitchen"))
        return new SingleJoin (new Column (leftSource, "IndustrialSector_PK"), new Column (rightSource, "Student_Detail_to_IndustrialSector_FK"));
      else if (relationMember == typeof (Kitchen).GetProperty ("Restaurant"))
        return new SingleJoin (new Column (leftSource, "Student_Detail_to_IndustrialSector_FK"), new Column (rightSource, "IndustrialSector_PK"));
      else if (relationMember == typeof (Cook).GetProperty ("Substitution"))
        return new SingleJoin (new Column (leftSource, "Student_to_OtherStudent_FK"), new Column (rightSource, "Student_PK"));
      else if (relationMember == typeof (Restaurant).GetProperty ("Cooks"))
        return new SingleJoin (new Column (leftSource, "Industrial_PK"), new Column (rightSource, "Student_to_IndustrialSector_FK"));
      else
      {
        string message =
            string.Format ("The member '{0}.{1}' does not identify a relation.", relationMember.DeclaringType.FullName, relationMember.Name);
        throw new UnmappedItemException (message);
      }
    }

    public object ProcessWhereParameter (object parameter)
    {
      var student = parameter as Cook;
      if (student != null)
        return student.ID;
      return parameter;
    }

    public MemberInfo GetPrimaryKeyMember (Type entityType)
    {
      if (entityType == typeof (Kitchen))
        return typeof (Kitchen).GetProperty ("ID");
      else if (entityType == typeof (Cook))
        return typeof (Cook).GetProperty ("ID");
      else if (entityType == typeof (Restaurant))
        return typeof (Restaurant).GetProperty ("ID");
      else
        return null;
    }

    private string GetTableName (FromClauseBase fromClause)
    {
      switch (fromClause.ItemType.Name)
      {
        case "Cook":
          return "studentTable";
        case "Kitchen":
          return "detailTable";
        case "Company":
          return "detailDetailTable";
        case "Restaurant":
          return "industrialTable";
        default:
          return null;
      }
    }

    private string GetRelatedTableName (MemberInfo relationMember)
    {
      if (relationMember == typeof (Kitchen).GetProperty ("Cook"))
        return "studentTable";
      else if (relationMember == typeof (Company).GetProperty ("MainKitchen"))
        return "detailTable";
      else if (relationMember == typeof (Company).GetProperty ("Restaurant"))
        return "industrialTable";
      else if (relationMember == typeof (Kitchen).GetProperty ("Restaurant"))
        return "industrialTable";
      else if (relationMember == typeof (Restaurant).GetProperty ("SubKitchen"))
        return "detailTable";
      else if (relationMember == typeof (Cook).GetProperty ("Substitution"))
        return "studentTable";
      else if (relationMember == typeof (Restaurant).GetProperty ("Cooks"))
        return "studentTable";
      else
        return null;
    }

    private string GetColumnName (MemberInfo member)
    {
      if (member.Name == "NonDBStringProperty" || member.Name == "NonDBBoolProperty")
        return null;
      else if (member == typeof (Kitchen).GetProperty ("Cook"))
        return null;
      else if (member == typeof (Company).GetProperty ("MainKitchen"))
        return null;
      else if (member == typeof (Company).GetProperty ("Restaurant"))
        return null;
      else if (member == typeof (Restaurant).GetProperty ("SubKitchen"))
        return null;
      else if (member == typeof (Kitchen).GetProperty ("Restaurant"))
        return "Student_Detail_to_IndustrialSector_FK";
      else if (member == typeof (Restaurant).GetProperty ("Cooks"))
        return null;
      else
        return member.Name + "Column";
    }
  }
}
