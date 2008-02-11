using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class AdditionalFromClause : FromClauseBase,IBodyClause
  {
    public AdditionalFromClause (IClause previousClause, ParameterExpression identifier, LambdaExpression fromExpression,
        LambdaExpression projectionExpression)
        : base (previousClause,identifier)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("fromExpression", fromExpression);
      ArgumentUtility.CheckNotNull ("projectionExpression", projectionExpression);

      FromExpression = fromExpression;
      ProjectionExpression = projectionExpression;
    }

    public LambdaExpression FromExpression { get; private set; }
    public LambdaExpression ProjectionExpression { get; private set; }


    public override void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitAdditionalFromClause (this);
    }

    public override Type GetQuerySourceType ()
    {
      return FromExpression.Body.Type;
    }

    public override FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, Expression partialFieldExpression, Expression fullFieldExpression)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);

      AdditionalFromClauseResolveVisitor visitor = new AdditionalFromClauseResolveVisitor (Identifier, ProjectionExpression.Parameters.ToArray());
      AdditionalFromClauseResolveVisitor.Result visitorResult = visitor.ParseAndReduce (partialFieldExpression, fullFieldExpression);

      if (visitorResult.FromIdentifierFound)
        return CreateFieldDescriptor (visitorResult.Member, databaseInfo, fullFieldExpression);
      else
        return PreviousClause.ResolveField (databaseInfo, visitorResult.ReducedExpression, fullFieldExpression);
    }
  }
}