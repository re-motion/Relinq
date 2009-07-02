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
using System.Linq.Expressions;
using Remotion.Collections;
using Remotion.Utilities;
using System.Linq;

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
    public OrderByClause ()
    {
      Orderings = new ObservableCollection<Ordering>();
      Orderings.ItemInserted += Orderings_ItemAdded;
      Orderings.ItemSet += Orderings_ItemAdded;
    }

    /// <summary>
    /// A collection of <see cref="Ordering"/>
    /// </summary>
    public ObservableCollection<Ordering> Orderings { get; private set; }

    public virtual void Accept (IQueryModelVisitor visitor, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      visitor.VisitOrderByClause (this, queryModel, index);
    }

    public void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);

      foreach (var ordering in Orderings)
      {
        ordering.TransformExpressions (transformation);
      }
    }

    public OrderByClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var result = new OrderByClause ();
      foreach (var ordering in Orderings)
      {
        var orderingClone = ordering.Clone (cloneContext);
        result.Orderings.Add (orderingClone);
      }

      return result;
    }

    private void Orderings_ItemAdded (object sender, ObservableCollectionChangedEventArgs<Ordering> e)
    {
      ArgumentUtility.CheckNotNull ("e.Item", e.Item);
    }

    IBodyClause IBodyClause.Clone (CloneContext cloneContext)
    {
      return Clone (cloneContext);
    }

    public override string ToString ()
    {
      var result = "orderby";
      if (Orderings.Count > 0)
        result = Orderings.Take (Orderings.Count - 1).Aggregate (result + " ", (s, o) => s + o + ", ") + Orderings[Orderings.Count - 1];

      return result;
    }
  }
}
