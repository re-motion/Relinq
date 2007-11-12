using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
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
  }
}