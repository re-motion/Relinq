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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Represents a lookup class from <see cref="IQuerySourceExpressionNode"/> to the respective <see cref="FromClauseBase"/>. This is filled
  /// when the <see cref="QueryParser"/> generates clauses from expression nodes, and it is used when resolving expressions whose data stems
  /// from <see cref="IQuerySourceExpressionNode"/> instances to associate the respective clause.
  /// </summary>
  public class QuerySourceClauseMapping
  {
    private readonly Dictionary<IQuerySourceExpressionNode, FromClauseBase> _lookup;

    public QuerySourceClauseMapping ()
    {
      _lookup = new Dictionary<IQuerySourceExpressionNode, FromClauseBase>();
    }

    public int Count
    {
      get { return _lookup.Count; }
    }

    public void AddMapping (IQuerySourceExpressionNode node, FromClauseBase fromClause)
    {
      ArgumentUtility.CheckNotNull ("node", node);
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      try
      {
        _lookup.Add (node, fromClause);
      }
      catch (ArgumentException)
      {
        throw new InvalidOperationException ("Node already has an associated clause.");
      }
    }

    public FromClauseBase GetFromClause (IQuerySourceExpressionNode node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      FromClauseBase fromClause;
      if (!_lookup.TryGetValue (node, out fromClause))
        throw new KeyNotFoundException ("Node has no associated clause.");

      return fromClause;
    }
  }
}