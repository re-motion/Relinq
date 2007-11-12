using System.Collections.Generic;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class QueryBody
  {
    private readonly ISelectGroupClause _SelectOrGroupClause;
    private readonly OrderingClause _orderingClause;
    private readonly List<IFromLetWhereClause> _fromLetWhere = new List<IFromLetWhereClause>();

    public QueryBody (ISelectGroupClause SelectOrGroupClause)
    {
      ArgumentUtility.CheckNotNull ("SelectOrGroupClause", SelectOrGroupClause);

      _SelectOrGroupClause = SelectOrGroupClause;
    }

    public QueryBody (ISelectGroupClause SelectOrGroupClause, OrderingClause orderingClause)
    {
      ArgumentUtility.CheckNotNull ("SelectOrGroupClause", SelectOrGroupClause);
      ArgumentUtility.CheckNotNull ("orderingClause", orderingClause);

      _SelectOrGroupClause = SelectOrGroupClause;
      _orderingClause = orderingClause;
    }

    public ISelectGroupClause ISelectOrGroupClause
    {
      get { return _SelectOrGroupClause; }
    }

    public OrderingClause OrderingClause
    {
      get { return _orderingClause; }
    }

    public IEnumerable<IFromLetWhereClause> FromLetWhere
    {
      get { return _fromLetWhere; }
    }

    public void Add (IFromLetWhereClause fromLetWhere)
    {
      _fromLetWhere.Add (fromLetWhere);
    }
  }
}