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
using System.Text;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="SqlStatementTextGenerator"/> generates sql-text from a given <see cref="SqlStatement"/>.
  /// </summary>
  public abstract class SqlStatementTextGenerator
  {
    public string Build (SqlStatement sqlStatement)
    {
      var sb = new StringBuilder();
      sb.Append ("SELECT ");
      BuildSelectPart ((SqlColumnListExpression) sqlStatement.SelectProjection, sb);  // TODO: Remove cast
      sb.Append (" FROM ");
      BuildFromPart (sqlStatement.FromExpression, sb);
      return sb.ToString();
    }

    protected abstract void BuildSelectPart (SqlColumnListExpression expression, StringBuilder sb); // TODO: Change parameter type to Expression
    protected abstract void BuildFromPart (SqlTable sqlTable, StringBuilder sb);
    
  }
}