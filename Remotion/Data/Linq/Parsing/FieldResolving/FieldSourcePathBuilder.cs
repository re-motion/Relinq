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
using System.Collections.Generic;
using System.Reflection;
using Remotion.Collections;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;

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
