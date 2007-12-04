using System;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Utilities;
using System.Linq.Expressions;

namespace Rubicon.Data.DomainObjects.Linq.Clauses
{
  public class SelectClause : ISelectGroupClause
  {
    private readonly LambdaExpression _projectionExpression;

    public SelectClause (IClause previousClause, LambdaExpression projectionExpression)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);

      PreviousClause = previousClause;
      _projectionExpression = projectionExpression;
    }

    public IClause PreviousClause { get; private set; }

    public LambdaExpression ProjectionExpression
    {
      get { return _projectionExpression; }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitSelectClause (this);
    }
  }
}