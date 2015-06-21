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
#if NET_3_5
using System.Diagnostics;
#endif
using System.Linq.Expressions;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses
{
  /// <summary>
  /// Represents a single ordering instruction in an <see cref="OrderByClause"/>.
  /// </summary>
  public sealed class Ordering
  {
    private Expression _expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ordering"/> class.
    /// </summary>
    /// <param name="expression">The expression used to order the data items returned by the query.</param>
    /// <param name="direction">The <see cref="OrderingDirection"/> to use for sorting.</param>
    public Ordering (Expression expression, OrderingDirection direction)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _expression = expression;
      OrderingDirection = direction;
    }

    /// <summary>
    /// Gets or sets the expression used to order the data items returned by the query.
    /// </summary>
    /// <value>The expression.</value>
#if NET_3_5
    [DebuggerDisplay ("{Remotion.Linq.Clauses.ExpressionVisitors.FormattingExpressionVisitor.Format (Expression),nq}")]
#endif
    public Expression Expression
    {
      get { return _expression; }
      set { _expression = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Gets or sets the direction to use for ordering data items.
    /// </summary>
    public OrderingDirection OrderingDirection { get; set; }

    /// <summary>
    /// Accepts the specified visitor by calling its <see cref="IQueryModelVisitor.VisitOrdering"/> method.
    /// </summary>
    /// <param name="visitor">The visitor to accept.</param>
    /// <param name="queryModel">The query model in whose context this clause is visited.</param>
    /// <param name="orderByClause">The <see cref="OrderByClause"/> in whose context this item is visited.</param>
    /// <param name="index">The index of this item in the <paramref name="orderByClause"/>'s <see cref="OrderByClause.Orderings"/> collection.</param>
    public void Accept (IQueryModelVisitor visitor, QueryModel queryModel, OrderByClause orderByClause, int index)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);

      visitor.VisitOrdering (this, queryModel, orderByClause, index);
    }

    /// <summary>
    /// Clones this item.
    /// </summary>
    /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext"/>.</param>
    /// <returns>A clone of this item.</returns>
    public Ordering Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var clone = new Ordering (Expression, OrderingDirection);
      return clone;
    }

    /// <summary>
    /// Transforms all the expressions in this item via the given <paramref name="transformation"/> delegate.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this
    /// item, and those expressions will be replaced with what the delegate returns.</param>
    public void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Expression = transformation (Expression);
    }

    public override string ToString ()
    {
      return Expression.BuildString() + (OrderingDirection == OrderingDirection.Asc ? " asc" : " desc");
    }
  }
}
