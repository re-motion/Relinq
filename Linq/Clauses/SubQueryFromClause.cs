using System;
using System.Linq.Expressions;
using Rubicon.Utilities;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.Clauses
{
  public class SubQueryFromClause : FromClauseBase, IBodyClause
  {
    private readonly SubQuery _fromSource;

    public SubQueryFromClause (IClause previousClause, ParameterExpression identifier, QueryModel subQuery, LambdaExpression projectionExpression)
        : base (previousClause, identifier)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("subQuery", subQuery);
      ArgumentUtility.CheckNotNull ("projectionExpression", projectionExpression);

      SubQueryModel = subQuery;
      ProjectionExpression = projectionExpression;

      _fromSource = new SubQuery (SubQueryModel, Identifier.Name);
    }

    public QueryModel SubQueryModel { get; private set; }
    public LambdaExpression ProjectionExpression { get; private set; }

    public override void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitSubQueryFromClause (this);
    }

    public override Type GetQuerySourceType ()
    {
      return null;
    }

    public override IFromSource GetFromSource (IDatabaseInfo databaseInfo)
    {
      return _fromSource;
    }
  }
}