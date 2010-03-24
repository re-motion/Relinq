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
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="ResolvedSubStatementTableInfo"/> represents the data source defined by a table of a subquery in a relational database.
  /// </summary>
  public class ResolvedSubStatementTableInfo : AbstractTableInfo, IResolvedTableInfo
  {
    private readonly Type _itemType;
    private readonly string _tableAlias;
    private readonly SqlStatement _sqlStatement;

    public ResolvedSubStatementTableInfo (Type itemType, string tableAlias, SqlStatement sqlStatement)
    {
      ArgumentUtility.CheckNotNull ("itemType", itemType);
      ArgumentUtility.CheckNotNullOrEmpty ("tableAlias", tableAlias);
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);
      
      _sqlStatement = sqlStatement;
      _tableAlias = tableAlias;
      _itemType = itemType;
    }

    public override Type ItemType
    {
      get { return _itemType; }
    }

    public string TableAlias
    {
      get { return _tableAlias;  }
    }

    public SqlStatement SqlStatement
    {
      get { return _sqlStatement; }
    }

    public override IResolvedTableInfo GetResolvedTableInfo ()
    {
      return this;
    }

    public override AbstractTableInfo Accept (ITableInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitSubStatementTableInfo(this);
    }
  }
}