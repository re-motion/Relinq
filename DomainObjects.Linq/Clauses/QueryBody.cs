using System.Collections.Generic;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Clauses
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

    public ISelectGroupClause SelectOrGroupClause
    {
      get { return _selectOrGroupClause; }
    }

    public OrderByClause OrderByClause
    {
      get { return _orderByClause; }
    }

    public IEnumerable<IFromLetWhereClause> FromLetWhereClauses
    {
      get { return _fromLetWhere; }
    }

    public void Add (IFromLetWhereClause fromLetWhere)
    {
      ArgumentUtility.CheckNotNull ("fromLetWhere", fromLetWhere);
      _fromLetWhere.Add (fromLetWhere);
    }

    public int FromLetWhereClauseCount
    {
      get { return _fromLetWhere.Count; }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitQueryBody (this);
    }
  }
}