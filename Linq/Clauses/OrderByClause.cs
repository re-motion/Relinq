using System;
using System.Collections.Generic;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class OrderByClause :IQueryElement,IBodyClause
  {
    private readonly List<OrderingClause> _orderingList = new List<OrderingClause>();

    public OrderByClause (OrderingClause ordering)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);
      _orderingList.Add (ordering);
    }

    public IEnumerable<OrderingClause> OrderingList
    {
      get { return _orderingList; }
    }

    public void Add(OrderingClause ordering)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);
      _orderingList.Add (ordering);
    }

    public int OrderByClauseCount
    {
      get { return _orderingList.Count; }
    }

    public IClause PreviousClause
    {
      get { return _orderingList[0].PreviousClause; }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitOrderByClause (this);
    }
  }
}