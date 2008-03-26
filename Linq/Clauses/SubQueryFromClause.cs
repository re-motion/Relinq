using System;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class SubQueryFromClause : FromClauseBase, IBodyClause
  {
    public SubQueryFromClause (IClause previousClause, ParameterExpression identifier,QueryExpression subQuery, LambdaExpression projectionExpression)
        : base (previousClause, identifier)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("subQuery", subQuery);
      ArgumentUtility.CheckNotNull ("projectionExpression", projectionExpression);

      SubQuery = subQuery;
      ProjectionExpression = projectionExpression;
    }

    public QueryExpression SubQuery { get; private set; }
    public LambdaExpression ProjectionExpression { get; private set; }

    public override void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitSubQueryFromClause (this);
    }

    public override Type GetQueriedEntityType ()
    {
      return null;
    }
  }
}