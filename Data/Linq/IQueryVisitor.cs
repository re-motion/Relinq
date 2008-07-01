using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq
{
  public interface IQueryVisitor
  {
    void VisitQueryModel (QueryModel queryModel);
    void VisitMainFromClause (MainFromClause fromClause);
    void VisitAdditionalFromClause (AdditionalFromClause fromClause);
    void VisitSubQueryFromClause (SubQueryFromClause clause);
    void VisitJoinClause (JoinClause joinClause);
    void VisitLetClause (LetClause letClause);
    void VisitWhereClause (WhereClause whereClause);
    void VisitOrderByClause (OrderByClause orderByClause);
    void VisitOrderingClause (OrderingClause orderingClause);
    void VisitSelectClause (SelectClause selectClause);
    void VisitGroupClause (GroupClause groupClause);
  }
}