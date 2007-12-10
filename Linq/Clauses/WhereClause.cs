using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class WhereClause : IFromLetWhereClause
  {
    private readonly LambdaExpression _boolExpression;
    
    public WhereClause (IClause previousClause,LambdaExpression boolExpression)
    {
      ArgumentUtility.CheckNotNull ("boolExpression", boolExpression);
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      _boolExpression = boolExpression;
      PreviousClause = previousClause;
    }

    public IClause PreviousClause { get; private set; }

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