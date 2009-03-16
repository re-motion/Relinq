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
  // TODO: Define whether OrderingClause should really be a clause; it's actually an element or part of OrderByClause, so maybe it should rather be
  // named OrderingElement or OrderingSpecification. (And then, it should not implement IClause. Its PreviousClause should be 
  // PreviousElement/Specification, and every element/specification should knows its OrderByClause.)
  /// <summary>
  /// Represents one expression of a order by in a linq query.
  /// </summary>
  public class OrderingClause : IClause
  {
    private readonly LambdaExpression _expression;
    private readonly OrderDirection _orderDirection;
    
    /// <summary>
    /// Initialize a new instance of <see cref="OrderingClause"/>
    /// </summary>
    /// <param name="previousClause">The previous clause of type <see cref="IClause"/> in the <see cref="QueryModel"/>.</param>
    /// <param name="expression">The expression from one part of a order by in a linq query.</param>
    /// <param name="direction"></param>
    public OrderingClause (IClause previousClause, LambdaExpression expression, OrderDirection direction)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      _expression = expression;
      _orderDirection = direction;
      PreviousClause = previousClause;
    }

    /// <summary>
    /// The previous clause of type <see cref="IClause"/> in the <see cref="QueryModel"/>.
    /// </summary>
    public IClause PreviousClause { get; set; }

    /// <summary>
    /// The expression from one part of a order by in a linq query.
    /// </summary>
    public LambdaExpression Expression
    {
      get { return _expression; }
    }

    public OrderDirection OrderDirection
    {
      get { return _orderDirection; }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitOrderingClause (this);
    }

    public QueryModel QueryModel { get; private set; }

    public void SetQueryModel (QueryModel model)
    {
      ArgumentUtility.CheckNotNull ("model", model);
      if (QueryModel != null)
        throw new InvalidOperationException ("QueryModel is already set");

      QueryModel = model;
    }

    public OrderingClause Clone (IClause newPreviousClause)
    {
      return new OrderingClause (newPreviousClause, Expression, OrderDirection);
    }
  }

  public enum OrderDirection
  {
    Asc,
    Desc
  }
}
