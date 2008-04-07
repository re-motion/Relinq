using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class LetClause : IBodyClause
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

      return resolver.ResolveField (GetNamedEvaluation(),Identifier,partialFieldExpression,fullFieldExpression);
    }

    public virtual NamedEvaluation GetNamedEvaluation ()
    {
      return new NamedEvaluation(Identifier.Name,Identifier.Name);
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitLetClause (this);
    }
  }
}