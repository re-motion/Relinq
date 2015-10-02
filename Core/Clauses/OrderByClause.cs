// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Linq.Expressions;
using Remotion.Linq.Utilities;
using Remotion.Utilities;
#if !NET_3_5
using System.Collections.ObjectModel;
using System.Collections.Specialized;
#endif
#if NET_3_5
using Remotion.Linq.Collections;
#endif

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
  public sealed class OrderByClause : IBodyClause
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderByClause"/> class.
    /// </summary>
    public OrderByClause ()
    {
      Orderings = new ObservableCollection<Ordering>();
      Orderings.CollectionChanged += Orderings_CollectionChanged;
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
    public void Accept (IQueryModelVisitor visitor, QueryModel queryModel, int index)
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
      var result = "orderby " + StringUtility.Join (", ", Orderings);

      return result;
    }

    private void Orderings_CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
    {
      ArgumentUtility.CheckNotNull ("e", e);
      ArgumentUtility.CheckItemsNotNullAndType ("e.NewItems", e.NewItems, typeof (Ordering));
    }
  }
}
