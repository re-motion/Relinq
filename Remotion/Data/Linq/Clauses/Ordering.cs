// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents one expression of a order by in a linq query.
  /// </summary>
  public class Ordering
  {
    private readonly LambdaExpression _expression;
    private readonly OrderingDirection _orderingDirection;
    
    /// <summary>
    /// Initialize a new instance of <see cref="Ordering"/>
    /// </summary>
    /// <param name="orderByClause">The <see cref="OrderByClause"/> associated with this <see cref="Ordering"/>.</param>
    /// <param name="expression">The expression from one part of a order by in a linq query.</param>
    /// <param name="direction"></param>
    public Ordering (OrderByClause orderByClause, LambdaExpression expression, OrderingDirection direction)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      ArgumentUtility.CheckNotNull ("expression", expression);

      _expression = expression;
      _orderingDirection = direction;
      OrderByClause = orderByClause;
    }

    /// <summary>
    /// The previous clause of type <see cref="IClause"/> in the <see cref="QueryModel"/>.
    /// </summary>
    public OrderByClause OrderByClause { get; set; }

    /// <summary>
    /// The expression from one part of a order by in a linq query.
    /// </summary>
    public LambdaExpression Expression
    {
      get { return _expression; }
    }

    public OrderingDirection OrderingDirection
    {
      get { return _orderingDirection; }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitOrdering (this);
    }

    public QueryModel QueryModel { get; private set; }

    public void SetQueryModel (QueryModel model)
    {
      ArgumentUtility.CheckNotNull ("model", model);
      if (QueryModel != null)
        throw new InvalidOperationException ("QueryModel is already set");

      QueryModel = model;
    }

    public Ordering Clone (OrderByClause newOrderByClause)
    {
      return new Ordering (newOrderByClause, Expression, OrderingDirection);
    }
  }
}
