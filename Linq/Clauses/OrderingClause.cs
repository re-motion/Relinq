using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class OrderingClause : IBodyClause
  {
    private readonly LambdaExpression _expression;
    private readonly OrderDirection _orderDirection;
    
    public OrderingClause (IClause previousClause, LambdaExpression expression, OrderDirection direction)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      _expression = expression;
      _orderDirection = direction;
      PreviousClause = previousClause;
    }

    public IClause PreviousClause { get; set;}

    public LambdaExpression Expression
    {
      get { return _expression; }
    }

    public OrderDirection OrderDirection{
      get { return _orderDirection; }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitOrderingClause (this);
    }

    // Note: As an optimization, one could go directly to this clause's OrderByClause instead of the PreviousClause for resolving fields.
    public FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, Expression partialFieldExpression, Expression fullFieldExpression)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);
      return PreviousClause.ResolveField (databaseInfo, partialFieldExpression, fullFieldExpression);
    }
  }

  public enum OrderDirection
  {
    Asc,
    Desc
  }
}