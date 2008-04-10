using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
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

    public QueryModel QueryModel { get; private set; }

    public ReadOnlyCollection<OrderingClause> OrderingList
    {
      get { return new ReadOnlyCollection<OrderingClause>(_orderingList); }
    }

    public void Add(OrderingClause ordering)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);
      _orderingList.Add (ordering);
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
    
    public void SetQueryModel (QueryModel model)
    {
      ArgumentUtility.CheckNotNull ("model", model);
      if (QueryModel != null)
        throw new InvalidOperationException("QueryModel is already set");
        QueryModel = model;
      
    }
    
  }
}