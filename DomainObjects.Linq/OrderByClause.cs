using System;
using System.Collections.Generic;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class OrderByClause
  {
    private readonly List<OrderingClause> _orderingList = new List<OrderingClause>();

    public OrderByClause (OrderingClause ordering)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);
      _orderingList.Add (ordering);
    }

    public IEnumerable<OrderingClause> OrderList
    {
      get { return _orderingList; }
    }

    public void Add(OrderingClause ordering)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);
      _orderingList.Add (ordering);
    }

  }
}