using System.Collections.Generic;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class QueryBody : IQueryElement
  {
    private readonly ISelectGroupClause _selectOrGroupClause;
    //private readonly OrderByClause _orderByClause;
    private readonly List<IBodyClause> _bodyClause = new List<IBodyClause>();

    public QueryBody (ISelectGroupClause selectOrGroupClause)
    {
      ArgumentUtility.CheckNotNull ("SelectOrGroupClause", selectOrGroupClause);
      _selectOrGroupClause = selectOrGroupClause;
    }
     

    public ISelectGroupClause SelectOrGroupClause
    {
      get { return _selectOrGroupClause; }
    }


    public IEnumerable<IBodyClause> BodyClauses
    {
      get { return _bodyClause; }
    }

    public void Add (IBodyClause clause)
    {
      ArgumentUtility.CheckNotNull ("clause", clause);
      _bodyClause.Add (clause);
    }

    public int BodyClauseCount
    {
      get { return _bodyClause.Count; }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitQueryBody (this);
    }
  }
}