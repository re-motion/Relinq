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
  /// <see cref="ResolvedSimpleTableInfo"/> represents the data source defined by a table in a relational database.
  /// </summary>
  public class ResolvedSimpleTableInfo : AbstractTableInfo, IResolvedTableInfo
  {
    private readonly Type _itemType;
    private readonly string _tableName;
    private readonly string _tableAlias;

    public ResolvedSimpleTableInfo (Type itemType, string tableName, string tableAlias)
    {
      ArgumentUtility.CheckNotNull ("itemType", itemType);
      ArgumentUtility.CheckNotNullOrEmpty ("tableName", tableName);
      ArgumentUtility.CheckNotNullOrEmpty ("tableAlias", tableAlias);

      _itemType = itemType;
      _tableName = tableName;
      _tableAlias = tableAlias;
    }

    public string TableName
    {
      get { return _tableName; }
    }

    public string TableAlias
    {
      get { return _tableAlias; }
    }

    public override Type ItemType
    {
      get { return _itemType; }
    }

    public override AbstractTableInfo Accept (ITableInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitSimpleTableInfo (this);
    }

    public override IResolvedTableInfo GetResolvedTableInfo ()
    {
      return this;
    }
  }
}