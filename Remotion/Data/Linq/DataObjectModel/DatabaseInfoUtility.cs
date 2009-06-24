// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public static class DatabaseInfoUtility
  {
    private static readonly InterlockedCache<Tuple<IDatabaseInfo, FromClauseBase>, Table> _tableCache =
        new InterlockedCache<Tuple<IDatabaseInfo, FromClauseBase>, Table>();

    public static Table GetTableForFromClause (IDatabaseInfo databaseInfo, FromClauseBase fromClause)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      return _tableCache.GetOrCreateValue (Tuple.NewTuple (databaseInfo, fromClause), key => GetTableFromDatabaseInfo (databaseInfo, fromClause));
    }

    private static Table GetTableFromDatabaseInfo (IDatabaseInfo databaseInfo, FromClauseBase fromClause)
    {
      string tableName = databaseInfo.GetTableName (fromClause);
      if (tableName == null)
      {
        Type queriedEntityType = fromClause.GetQuerySourceType();
        string message = string.Format ("The from clause with identifier {0} and query source type {1} does not identify a queryable table.",
          fromClause.ItemName, queriedEntityType != null ? queriedEntityType.FullName : "<null>");
        throw new ArgumentException (message, "fromClause");
      }
      else
        return new Table (tableName, fromClause.ItemName);
    }

    public static Table GetRelatedTable (IDatabaseInfo databaseInfo, MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      string tableName = databaseInfo.GetRelatedTableName (relationMember);
      if (tableName == null)
      {
        string message =
            string.Format ("The member '{0}.{1}' does not identify a relation.", relationMember.DeclaringType.FullName, relationMember.Name);
        throw new InvalidOperationException (message);
      }
      else
        return new Table (tableName, null);
    }

    public static Tuple<string, string> GetJoinColumnNames (IDatabaseInfo databaseInfo, MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      Tuple<string, string> columns = databaseInfo.GetJoinColumnNames (relationMember);
      if (columns == null)
      {
        string message =
            string.Format ("The member '{0}.{1}' does not identify a relation.", relationMember.DeclaringType.FullName, relationMember.Name);
        throw new InvalidOperationException (message);
      }
      else
        return columns;
    }

    public static bool IsRelationMember (IDatabaseInfo databaseInfo, MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("member", member);

      return databaseInfo.GetRelatedTableName (member) != null;
    }

    public static bool IsVirtualColumn (IDatabaseInfo databaseInfo, MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      return (IsRelationMember (databaseInfo, member)) && (databaseInfo.GetColumnName (member) == null);
    }
    
    public static Column? GetColumn (IDatabaseInfo databaseInfo, IColumnSource columnSource, MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      string columnName = member == null ? "*" : databaseInfo.GetColumnName (member);
      if (columnName == null)
        return null;
      else if (!columnSource.IsTable && member == null)
        return new Column (columnSource, null);
      else
        return new Column (columnSource, columnName);
      //return new Column (columnSource, columnName, ReflectionUtility.GetFieldOrPropertyType (member));
    }
    
    public static MemberInfo GetPrimaryKeyMember (IDatabaseInfo databaseInfo, Type entityType)
    {
      MemberInfo primaryKeyMember = databaseInfo.GetPrimaryKeyMember (entityType);
      if (primaryKeyMember == null)
      {
        var message = string.Format ("The primary key member of type '{0}' cannot be determined because it is no entity type.", entityType.FullName);
        throw new InvalidOperationException (message);
      }
      else
        return primaryKeyMember;
    }
  }
}
