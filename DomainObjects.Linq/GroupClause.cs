using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class GroupClause
  {

    private readonly Expression _groupExpression;
    private readonly Expression _byExpression;


    public GroupClause (Expression groupExpression, Expression byExpression)
    {
      ArgumentUtility.CheckNotNull ("groupExpression", groupExpression);
      ArgumentUtility.CheckNotNull ("byExpression", byExpression);

      _groupExpression = groupExpression;
      _byExpression = byExpression;
    }


    public Expression groupExpression
    {
      get { return _groupExpression; }
    }

    public Expression byExpression
    {
      get { return _byExpression; }
    }
  }
}