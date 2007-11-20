using System.Linq.Expressions;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Clauses
{
  public class WhereClause : IFromLetWhereClause
  {
    private readonly LambdaExpression _boolExpression;

    public WhereClause (LambdaExpression boolExpression)
    {
      ArgumentUtility.CheckNotNull ("boolExpression", boolExpression);
      _boolExpression = boolExpression;
    }

    public LambdaExpression BoolExpression
    {
      get { return _boolExpression; }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitWhereClause (this);
    }
  }
}