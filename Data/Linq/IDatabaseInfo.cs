/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Reflection;
using Remotion.Data.Linq.Clauses;
using Remotion.Collections;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// The interface has to be implemented so that the linq provider can use the underlying system to get information about the data source.
  /// </summary>
  public interface IDatabaseInfo
  {
    string GetTableName (FromClauseBase fromClause);
    string GetRelatedTableName (MemberInfo relationMember);
    string GetColumnName (MemberInfo member);
    Tuple<string, string> GetJoinColumnNames (MemberInfo relationMember);
    object ProcessWhereParameter (object parameter);
    MemberInfo GetPrimaryKeyMember (Type entityType);
    bool IsTableType (Type type);
  }
}
