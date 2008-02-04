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

    public FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, Expression partialFieldExpression, Expression fullFieldExpression)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);

      return PreviousClause.ResolveField (databaseInfo, partialFieldExpression, fullFieldExpression);
    }
  }
}