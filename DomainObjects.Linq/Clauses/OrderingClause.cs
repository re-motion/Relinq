using System;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Clauses
{
  public class OrderingClause : IClause
  {
    private readonly Expression _expression;
    private readonly OrderDirection _orderDirection;
    
    public OrderingClause (IClause previousClause,Expression expression, OrderDirection direction)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      _expression = expression;
      _orderDirection = direction;
      PreviousClause = previousClause;
    }

    public IClause PreviousClause { get; private set; }

    public Expression Expression
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
  }

  public enum OrderDirection
  {
    Asc,
    Desc
  }
}