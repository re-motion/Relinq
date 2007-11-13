using System.Linq.Expressions;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Clauses
{
  public class WhereClause : IFromLetWhereClause
  {
    private readonly Expression _boolExpression;

    public WhereClause (Expression boolExpression)
    {
      ArgumentUtility.CheckNotNull ("boolExpression", boolExpression);
      _boolExpression = boolExpression;
    }

    public Expression BoolExpression
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