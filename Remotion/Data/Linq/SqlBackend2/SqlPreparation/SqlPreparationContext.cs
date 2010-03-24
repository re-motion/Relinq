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
using System.Collections.Generic;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlPreparationContext"/> is a helper class which maps <see cref="IQuerySource"/> to <see cref="SqlTable"/>.
  /// </summary>
  public class SqlPreparationContext
  {
    private readonly Dictionary<IQuerySource, SqlTableBase> _mapping;

    public SqlPreparationContext ()
    {
      _mapping = new Dictionary<IQuerySource, SqlTableBase>();
    }

    public int QuerySourceMappingCount
    {
      get { return _mapping.Count; }
    }

    public void AddQuerySourceMapping (IQuerySource source, SqlTableBase sqlTable)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      _mapping.Add (source, sqlTable);
    }

    public SqlTableBase GetSqlTableForQuerySource (IQuerySource source)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      return _mapping[source];
    }
  }
}