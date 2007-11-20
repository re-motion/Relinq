using System;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Utilities;
using System.Linq.Expressions;

namespace Rubicon.Data.DomainObjects.Linq.Clauses
{
  public class SelectClause : ISelectGroupClause
  {
    private readonly LambdaExpression[] _projectionExpressions;

    public SelectClause (LambdaExpression[] projectionExpression)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("projectionExpression", projectionExpression);

      _projectionExpressions = projectionExpression;
    }

    public LambdaExpression[] ProjectionExpressions
    {
      get { return _projectionExpressions; }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitSelectClause (this);
    }
  }
}