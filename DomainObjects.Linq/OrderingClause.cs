using System;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class OrderingClause : IQueryElement
  {
    private readonly Expression _expression;
    private readonly OrderDirection _orderDirection;
    
    public OrderingClause (Expression expression, OrderDirection direction)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _expression = expression;
      _orderDirection = direction;
    }

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