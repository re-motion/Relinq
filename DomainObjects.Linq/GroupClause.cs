using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class GroupClause : ISelectGroupClause,IQueryElement
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


    public Expression GroupExpression
    {
      get { return _groupExpression; }
    }

    public Expression ByExpression
    {
      get { return _byExpression; }
    }

    public void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitGroupClause (this);
    }
  }
}