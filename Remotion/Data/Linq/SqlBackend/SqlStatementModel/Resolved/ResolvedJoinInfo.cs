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
  /// <see cref="ResolvedJoinInfo"/> represents a join between two database tables.
  /// </summary>
  public class ResolvedJoinInfo : AbstractJoinInfo
  {
    private readonly IResolvedTableInfo _foreignTableInfo;
    private readonly SqlColumnExpression _primaryColumn;
    private readonly SqlColumnExpression _foreignColumn;

    public ResolvedJoinInfo (IResolvedTableInfo foreignTableInfo, SqlColumnExpression primaryColumn, SqlColumnExpression foreignColumn)
    {
      ArgumentUtility.CheckNotNull ("foreignTableInfo", foreignTableInfo);
      ArgumentUtility.CheckNotNull ("primaryColumn", primaryColumn);
      ArgumentUtility.CheckNotNull ("foreignColumn", foreignColumn);
      
      _foreignTableInfo = foreignTableInfo;
      _primaryColumn = primaryColumn;
      _foreignColumn = foreignColumn;
    }

    public override Type ItemType
    {
      get { return ((AbstractTableInfo) _foreignTableInfo).ItemType; }
    }

    public IResolvedTableInfo ForeignTableInfo
    {
      get { return _foreignTableInfo; }
    }

    public SqlColumnExpression PrimaryColumn
    {
      get { return _primaryColumn; }
    }

    public SqlColumnExpression ForeignColumn
    {
      get { return _foreignColumn; }
    }

    public override AbstractJoinInfo Accept (IJoinInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitResolvedJoinInfo (this);
    }

   public override IResolvedTableInfo GetResolvedTableInfo ()
    {
      return ForeignTableInfo;
    }
  }
}