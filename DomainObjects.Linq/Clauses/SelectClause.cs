using System;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Utilities;
using System.Linq.Expressions;

namespace Rubicon.Data.DomainObjects.Linq.Clauses
{
  public class SelectClause : ISelectGroupClause
  {
    private readonly LambdaExpression _projectionExpression;

    public SelectClause (LambdaExpression projectionExpression)
    {
      _projectionExpression = projectionExpression;
    }

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