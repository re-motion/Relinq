using Rubicon.Data.DomainObjects.Linq.Clauses;

namespace Rubicon.Data.DomainObjects.Linq
{
  public interface IQueryVisitor
  {
    void VisitQueryExpression (QueryExpression queryExpression);
    void VisitFromClause (FromClause fromClause);
    void VisitJoinClause (JoinClause joinClause);
    void VisitLetClause (LetClause letClause);
    void VisitWhereClause (WhereClause whereClause);
    void VisitOrderByClause (OrderByClause orderByClause);
    void VisitOrderingClause (OrderingClause orderingClause);
    void VisitSelectClause (SelectClause selectClause);
    void VisitGroupClause (GroupClause groupClause);
    void VisitQueryBody (QueryBody queryBody);

  }
}