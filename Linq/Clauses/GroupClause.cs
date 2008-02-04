using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class GroupClause : ISelectGroupClause
  {

    private readonly Expression _groupExpression;
    private readonly Expression _byExpression;


    public GroupClause (IClause previousClause,Expression groupExpression, Expression byExpression)
    {
      ArgumentUtility.CheckNotNull ("groupExpression", groupExpression);
      ArgumentUtility.CheckNotNull ("byExpression", byExpression);
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);

      _groupExpression = groupExpression;
      _byExpression = byExpression;
      PreviousClause = previousClause;
    }

    public IClause PreviousClause { get; private set; }

    public FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, Expression partialFieldExpression, Expression fullFieldExpression)
    {
      throw new System.NotImplementedException();
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