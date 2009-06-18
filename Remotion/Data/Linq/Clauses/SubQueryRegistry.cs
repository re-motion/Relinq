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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Keeps a list of subqueries in order to be able to update their <see cref="QueryModel.ParentQuery"/> altogether.
  /// </summary>
  public class SubQueryRegistry
  {
    private readonly List<QueryModel> _subQueries = new List<QueryModel> ();

    public void Add (QueryModel subQuery)
    {
      ArgumentUtility.CheckNotNull ("subQuery", subQuery);
      _subQueries.Add (subQuery);
    }

    public void UpdateAllParentQueries (QueryModel parentQueryModel)
    {
      ArgumentUtility.CheckNotNull ("parentQueryModel", parentQueryModel);

      foreach (var queryModel in _subQueries)
        queryModel.SetParentQuery (parentQueryModel);
    }

    public bool Contains (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      return _subQueries.Contains (queryModel);
    }
  }
}