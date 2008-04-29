using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.Parsing.Details;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class LetClause : IBodyClause, IResolveableClause
  {
    private readonly ParameterExpression _identifier;
    private readonly Expression _expression;

    public LetClause (IClause previousClause, ParameterExpression identifier, Expression expression, 
      LambdaExpression projectionExpression)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("projectionExpression", projectionExpression);
      
      
      _identifier = identifier;
      _expression = expression;
      PreviousClause = previousClause;
      ProjectionExpression = projectionExpression;
    }

    public IClause PreviousClause { get; private set; }
    public LambdaExpression ProjectionExpression { get; private set; }
    public QueryModel QueryModel { get; private set; }

    public Expression Expression
    {
      get { return _expression; }
    }

    public ParameterExpression Identifier
    {
      get { return _identifier; }
    }

    public FieldDescriptor ResolveField (ClauseFieldResolver resolver, Expression partialFieldExpression, Expression fullFieldExpression)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);

      return resolver.ResolveField (GetColumnSource (resolver.DatabaseInfo), Identifier, partialFieldExpression, fullFieldExpression);
    }

    public virtual LetColumnSource GetColumnSource (IDatabaseInfo databaseInfo)
    { 
      // TODO: IsTable should also be true if the let clause constructs an object, eg: let x = new {o.ID, o.OrderNumber}
      return new LetColumnSource (Identifier.Name, databaseInfo.IsTableType (Identifier.Type));
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitLetClause (this);
    }

    public void SetQueryModel (QueryModel model)
    {
      ArgumentUtility.CheckNotNull ("model", model);
      if (QueryModel != null)
        throw new InvalidOperationException("QueryModel is already set");
        QueryModel = model;
      
    }
  }
}