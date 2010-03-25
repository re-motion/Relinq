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
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// <see cref="UnresolvedTableInfo"/> holds a <see cref="ConstantExpression"/> representing the data source defined by a LINQ query.
  /// </summary>
  public class UnresolvedTableInfo : AbstractTableInfo
  {
    private readonly Type _itemType;

    public UnresolvedTableInfo (Type itemType)
    {
      ArgumentUtility.CheckNotNull ("itemType", itemType);

      _itemType = itemType;
    }

    public override Type ItemType
    {
      get { return _itemType;  }
    }

    public override AbstractTableInfo Accept (ITableInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitUnresolvedTableInfo (this);
    }

    public override IResolvedTableInfo GetResolvedTableInfo ()
    {
      throw new InvalidOperationException ("This table has not yet been resolved; call the resolution step first.");
    }
  }
}