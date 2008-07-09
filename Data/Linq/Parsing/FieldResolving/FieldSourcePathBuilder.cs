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
using System.Collections.Generic;
using System.Reflection;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  public class FieldSourcePathBuilder
  {
    public FieldSourcePath BuildFieldSourcePath (IDatabaseInfo databaseInfo, JoinedTableContext context, IColumnSource firstSource, IEnumerable<MemberInfo> joinMembers)
    {
      List<SingleJoin> joins = new List<SingleJoin>();

      IColumnSource lastSource = firstSource;
      foreach (MemberInfo member in joinMembers)
      {
        FieldSourcePath pathSoFar = new FieldSourcePath (firstSource, joins);
        try
        {
          Table relatedTable = context.GetJoinedTable (databaseInfo, pathSoFar, member);
          Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumnNames (databaseInfo, member);

          //Column leftColumn = new Column (lastSource, joinColumns.A, ReflectionUtility.GetFieldOrPropertyType (member));
          //Column rightColumn = new Column (relatedTable, joinColumns.B, ReflectionUtility.GetFieldOrPropertyType (member));
          Column leftColumn = new Column (lastSource, joinColumns.A);
          Column rightColumn = new Column (relatedTable, joinColumns.B);

          joins.Add (new SingleJoin (leftColumn, rightColumn));
          lastSource = relatedTable;
        }
        catch (Exception ex)
        {
          throw new FieldAccessResolveException (ex.Message, ex);
        }
      }

      return new FieldSourcePath(firstSource, joins);
    }
  }
}
