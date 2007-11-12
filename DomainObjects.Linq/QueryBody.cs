using System.Collections.Generic;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class QueryBody : IQueryElement
  {
    private readonly ISelectGroupClause _selectOrGroupClause;
    private readonly OrderByClause _orderByClause;
    private readonly List<IFromLetWhereClause> _fromLetWhere = new List<IFromLetWhereClause>();

    public QueryBody (ISelectGroupClause selectOrGroupClause)
    {
      ArgumentUtility.CheckNotNull ("SelectOrGroupClause", selectOrGroupClause);
      _selectOrGroupClause = selectOrGroupClause;
    }

    public QueryBody (ISelectGroupClause selectOrGroupClause, OrderByClause orderByClause) 
      : this (selectOrGroupClause)
  {
    ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
    _orderByClause = orderByClause;
    }

    public ISelectGroupClause ISelectOrGroupClause
    {
      get { return _selectOrGroupClause; }
    }

    public OrderByClause OrderByClause
    {
      get { return _orderByClause; }
    }

    public IEnumerable<IFromLetWhereClause> FromLetWhere
    {
      get { return _fromLetWhere; }
    }

    public void Add (IFromLetWhereClause fromLetWhere)
    {
      ArgumentUtility.CheckNotNull ("fromLetWhere", fromLetWhere);
      _fromLetWhere.Add (fromLetWhere);
    }

    public int FromLetWhereCount
    {
      get { return _fromLetWhere.Count; }
    }

    public void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitQueryBody (this);
    }
  }
}