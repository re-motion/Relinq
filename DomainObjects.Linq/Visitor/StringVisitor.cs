using System.Text;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Visitor
{
  public class StringVisitor : IQueryVisitor
  {
    private readonly StringBuilder _sb = null;

    public StringVisitor()
    {
      _sb = new StringBuilder();
    }

    #region IQueryVisitor Members

    public void VisitQueryExpression (QueryExpression queryExpression)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);

      queryExpression.FromClause.Accept (this);
      queryExpression.QueryBody.Accept (this);
      
    }

    public void VisitFromClause (FromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      _sb.AppendFormat ("from {0} {1} in {2} ", fromClause.Identifier.Type, fromClause.Identifier.Name, fromClause.QuerySource);

      foreach (JoinClause jc in fromClause.JoinClauses)
      {
        jc.Accept (this);
      }
    }

    public void VisitJoinClause (JoinClause joinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      _sb.AppendFormat("join {0} {1} in {2} on {3} equals {4} into {5} ",
        joinClause.Identifier.Type,joinClause.Identifier,joinClause.InExpression,
        joinClause.OnExpression,joinClause.EqualityExpression,joinClause.IntoIdentifier);
    }

    public void VisitLetClause (LetClause letClause)
    {
      ArgumentUtility.CheckNotNull ("letClause", letClause);

      _sb.AppendFormat ("let {0} = {1} ", letClause.Identifier, letClause.Expression);
    }

    public void VisitWhereClause (WhereClause whereClause)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);

      _sb.AppendFormat ("where {0} ", whereClause.BoolExpression);
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      _sb.Append ("orderby ");
      foreach (OrderingClause oC in orderByClause.OrderingList)
      {
        oC.Accept(this); 
      }
    }

    public void VisitOrderingClause (OrderingClause orderingClause)
    {
      ArgumentUtility.CheckNotNull ("orderingClause", orderingClause);

      switch( orderingClause.OrderDirection)
      {
        case OrderDirection.Asc:
          _sb.AppendFormat ("{0} ascending ", orderingClause.Expression);
          break;
        case OrderDirection.Desc:
          _sb.AppendFormat ("{0} descending ", orderingClause.Expression);
          break;
      }
      
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      _sb.AppendFormat ("select {0}", selectClause.Expression.ToString());
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
      ArgumentUtility.CheckNotNull ("groupClause", groupClause);
      _sb.AppendFormat ("group {0} by {1}", groupClause.GroupExpression, groupClause.ByExpression);
      
    }

    public void VisitQueryBody (QueryBody queryBody)
    {
      ArgumentUtility.CheckNotNull ("queryBody", queryBody);
      
      queryBody.SelectOrGroupClause.Accept (this);

      if (queryBody.OrderByClause != null)      
        queryBody.OrderByClause.Accept (this);
      foreach (IFromLetWhereClause fromLetWhereClause in queryBody.FromLetWhereClauses)
      {
        fromLetWhereClause.Accept(this);
      }
    }

    #endregion

    public override string ToString ()
    {
      return _sb.ToString();
    }
  }
}