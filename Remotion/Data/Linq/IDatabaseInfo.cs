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
using Remotion.Data.Linq.Clauses;
using Remotion.Collections;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// The interface has to be implemented so that the linq provider can use the underlying system to get information of the data source.
  /// </summary>
  public interface IDatabaseInfo
  {
    /// <summary>
    /// Has to be implemented to get the table name of the given clause.
    /// </summary>
    /// <param name="fromClause">The clause identifies the query source.</param>
    /// <returns>The table name as string.</returns>
    string GetTableName (FromClauseBase fromClause);
    
    /// <summary>
    /// Has to be implemented to get the table name of the n-side of a relation.
    /// </summary>
    /// <param name="relationMember"><see cref="MemberInfo"/></param>
    /// <returns>The table name of the related side of a join as string.</returns>
    string GetRelatedTableName (MemberInfo relationMember);

    /// <summary>
    /// Has to be implemented to get the column name of a given member.
    /// </summary>
    /// <param name="member"><see cref="MemberInfo"/></param>
    /// <returns>The column name as string.</returns>
    string GetColumnName (MemberInfo member);

    /// <summary>
    /// Has to be implemented to get affected columns of a join.
    /// </summary>
    /// <param name="relationMember"><see cref="MemberInfo"/></param>
    /// <returns>A tuple which containes the left and right side of a join. It must return the name of the primary and foreign key.</returns>
    Tuple<string, string> GetJoinColumnNames (MemberInfo relationMember);
    
    /// <summary>
    /// Has to be implemented to get value of a parameter in a where condition.
    /// </summary>
    /// <param name="parameter">The parameter in a where condition.</param>
    /// <returns>The value of the given where parameter.</returns>
    object ProcessWhereParameter (object parameter);
    
    /// <summary>
    /// Has to be implemented to get primary key member of a given entity.
    /// </summary>
    /// <param name="entityType">The type of the queried entity.</param>
    /// <returns><see cref="MemberInfo"/> of the primary key.</returns>
    MemberInfo GetPrimaryKeyMember (Type entityType);
    
    /// <summary>
    /// Has to be implemented to check whether a given type is a table.
    /// </summary>
    /// <param name="type">The type of a queried entity.</param>
    /// <returns>Boolean value depending on the evaluation.</returns>
    bool IsTableType (Type type);
  }
}
