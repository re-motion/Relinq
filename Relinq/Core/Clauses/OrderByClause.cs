// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using Remotion.Linq.Collections;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses
{
  /// <summary>
  /// Represents the orderby part of a query, ordering data items according to some <see cref="Orderings"/>.
  /// </summary>
  /// <example>
  /// In C#, the whole "orderby" clause in the following sample (including two orderings) corresponds to an <see cref="OrderByClause"/>:
  /// <ode>
  /// var query = from s in Students
  ///             orderby s.Last, s.First
  ///             select s;
  /// </ode>
  /// </example>
  public class OrderByClause : IBodyClause
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderByClause"/> class.
    /// </summary>
    public OrderByClause ()
    {
      Orderings = new ObservableCollection<Ordering>();
      Orderings.ItemInserted += Orderings_ItemAdded;
      Orderings.ItemSet += Orderings_ItemAdded;
    }

    /// <summary>
    /// Gets the <see cref="Ordering"/> instances that define how to sort the items coming from previous clauses. The order of the 
    /// <see cref="Orderings"/> in the collection defines their priorities. For example, { LastName, FirstName } would sort all items by
    /// LastName, and only those items that have equal LastName values would be sorted by FirstName.
    /// </summary>
    public ObservableCollection<Ordering> Orderings { get; private set; }

    /// <summary>
    /// Accepts the specified visitor by calling its <see cref="IQueryModelVisitor.VisitOrderByClause"/> method.
    /// </summary>
    /// <param name="visitor">The visitor to accept.</param>
    /// <param name="queryModel">The query model in whose context this clause is visited.</param>
    /// <param name="index">The index of this clause in the <paramref name="queryModel"/>'s <see cref="QueryModel.BodyClauses"/> collection.</param>
    public virtual void Accept (IQueryModelVisitor visitor, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      visitor.VisitOrderByClause (this, queryModel, index);
    }

    /// <summary>
    /// Transforms all the expressions in this clause and its child objects via the given <paramref name="transformation"/> delegate.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this
    /// clause, and those expressions will be replaced with what the delegate returns.</param>
    public void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);

      foreach (var ordering in Orderings)
        ordering.TransformExpressions (transformation);
    }

    /// <summary>
    /// Clones this clause.
    /// </summary>
    /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext"/>.</param>
    /// <returns>A clone of this clause.</returns>
    public OrderByClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var result = new OrderByClause();
      foreach (var ordering in Orderings)
      {
        var orderingClone = ordering.Clone (cloneContext);
        result.Orderings.Add (orderingClone);
      }

      return result;
    }

    IBodyClause IBodyClause.Clone (CloneContext cloneContext)
    {
      return Clone (cloneContext);
    }

    public override string ToString ()
    {
      var result = "orderby " + SeparatedStringBuilder.Build (", ", Orderings);

      return result;
    }

    private void Orderings_ItemAdded (object sender, ObservableCollectionChangedEventArgs<Ordering> e)
    {
      ArgumentUtility.CheckNotNull ("e.Item", e.Item);
    }
  }
}
