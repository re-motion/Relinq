using System;
using System.Collections.Generic;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class OrderBy
  {
    private readonly List<Ordering> _orderList = new List<Ordering>();

    public OrderBy (Ordering ordering)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);
      _orderList.Add (ordering);
    }

    public IEnumerable<Ordering> OrderList
    {
      get { return _orderList; }
    }

    public void Add(Ordering ordering)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);
      _orderList.Add (ordering);
    }

  }
}