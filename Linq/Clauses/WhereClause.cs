using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.TreeEvaluation;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class WhereClause : IBodyClause
  {
    private readonly LambdaExpression _boolExpression;
    private LambdaExpression _simplifiedBoolExpression;
    
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

    public LambdaExpression GetSimplifiedBoolExpression()
    {
      if (_simplifiedBoolExpression == null)
        _simplifiedBoolExpression = (LambdaExpression) new PartialTreeEvaluator (BoolExpression).GetEvaluatedTree ();
      return _simplifiedBoolExpression;
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitWhereClause (this);
    }

    public virtual FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, Expression fieldAccessExpression, Expression fullFieldExpression)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);
      return PreviousClause.ResolveField (databaseInfo, fieldAccessExpression, fullFieldExpression);
    }
  }
}