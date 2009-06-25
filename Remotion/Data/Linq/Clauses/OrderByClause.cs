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
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents the whole order by part of a linq query.
  /// example: orderby expression
  /// </summary>
  public class OrderByClause : IBodyClause
  {
    /// <summary>
    /// Initialize a new instance of <see cref="OrderByClause"/>
    /// </summary>
    /// <param name="previousClause">The previous clause of type <see cref="IClause"/> in the <see cref="QueryModel"/>.</param>
    public OrderByClause (IClause previousClause)
    {
      PreviousClause = previousClause;
      Orderings = new ObservableCollection<Ordering>();
      Orderings.ItemInserted += CheckForNullValues;
      Orderings.ItemSet += CheckForNullValues;
    }

    /// <summary>
    /// A collection of <see cref="Ordering"/>
    /// </summary>
    public ObservableCollection<Ordering> Orderings { get; private set; }

    public IClause PreviousClause { get; private set; }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitOrderByClause (this);
    }
    
    public OrderByClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var result = new OrderByClause (null);
      cloneContext.ClonedClauseMapping.AddMapping (this, result);
      foreach (var ordering in Orderings)
      {
        var orderingClone = ordering.Clone (cloneContext);
        result.Orderings.Add (orderingClone);
      }

      return result;
    }

    private void CheckForNullValues (object sender, ObservableCollectionChangedEventArgs<Ordering> e)
    {
      ArgumentUtility.CheckNotNull ("e.Item", e.Item);
    }

    IBodyClause IBodyClause.Clone (CloneContext cloneContext)
    {
      return Clone (cloneContext);
    }
  }
}
