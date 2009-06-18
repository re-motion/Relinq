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
using System.Collections.ObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents the whole order by part of a linq query.
  /// example: orderby expression
  /// </summary>
  public class OrderByClause : IBodyClause
  {
    private readonly List<Ordering> _orderings = new List<Ordering>();
    
    /// <summary>
    /// Initialize a new instance of <see cref="OrderByClause"/>
    /// </summary>
    /// <param name="previousClause">The previous clause of type <see cref="IClause"/> in the <see cref="QueryModel"/>.</param>
    public OrderByClause (IClause previousClause)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      PreviousClause = previousClause;
    }

    public QueryModel QueryModel { get; private set; }

    /// <summary>
    /// A collection of <see cref="Ordering"/>
    /// </summary>
    public ReadOnlyCollection<Ordering> OrderingList
    {
      get { return new ReadOnlyCollection<Ordering>(_orderings); }
    }

    public void AddOrdering(Ordering ordering)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);
      _orderings.Add (ordering);
    }

    public IClause PreviousClause { get; private set; }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitOrderByClause (this);
    }
    
    public void SetQueryModel (QueryModel model)
    {
      ArgumentUtility.CheckNotNull ("model", model);
      if (QueryModel != null)
        throw new InvalidOperationException("QueryModel is already set");
      
      QueryModel = model;
    }

    public OrderByClause Clone (IClause newPreviousClause, ClonedClauseMapping clonedClauseMapping)
    {
      ArgumentUtility.CheckNotNull ("newPreviousClause", newPreviousClause);
      ArgumentUtility.CheckNotNull ("clonedClauseMapping", clonedClauseMapping);

      var clone = new OrderByClause (newPreviousClause);

      foreach (var ordering in _orderings)
      {
        var orderingClone = ordering.Clone (clone, clonedClauseMapping);
        clone.AddOrdering (orderingClone);
      }

      return clone;
    }

    IBodyClause IBodyClause.Clone (IClause newPreviousClause, ClonedClauseMapping clonedClauseMapping)
    {
      return Clone (newPreviousClause, new ClonedClauseMapping());
    }
  }
}
