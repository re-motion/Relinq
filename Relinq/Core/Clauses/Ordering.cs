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
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses
{
  /// <summary>
  /// Represents a single ordering instruction in an <see cref="OrderByClause"/>.
  /// </summary>
  public class Ordering
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
    [DebuggerDisplay ("{Remotion.Data.Linq.Clauses.ExpressionTreeVisitors.FormattingExpressionTreeVisitor.Format (Expression),nq}")]
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
    public virtual void Accept (IQueryModelVisitor visitor, QueryModel queryModel, OrderByClause orderByClause, int index)
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
    virtual public Ordering Clone (CloneContext cloneContext)
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
      return FormattingExpressionTreeVisitor.Format (Expression) + (OrderingDirection == OrderingDirection.Asc ? " asc" : " desc");
    }
  }
}
