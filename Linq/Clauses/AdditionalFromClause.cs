using System;
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

      MemberExpression memberExpression =
          ParserUtility.GetTypedExpression<MemberExpression> (partialFieldExpression, "resolving field access", fullFieldExpression);

      // remove transparent identifier added by the SelectMany call corresponding to this clause
      Expression reducedExpression = new MemberAccessReducingTreeVisitor ().ReduceInnermostMemberExpression (memberExpression);

      switch (reducedExpression.NodeType)
      {
        case ExpressionType.MemberAccess:
          return ResolveMemberAccess ((MemberExpression) reducedExpression, databaseInfo, fullFieldExpression);
        case ExpressionType.Parameter:
          return ResolveParameter ((ParameterExpression) reducedExpression, databaseInfo, fullFieldExpression);
        default:
          throw ParserUtility.CreateParserException ("MemberExpression or ParameterExpression", reducedExpression, "resolving field access",
                fullFieldExpression);
      }
    }

    private FieldDescriptor ResolveMemberAccess (MemberExpression memberExpression, IDatabaseInfo databaseInfo, Expression fullFieldExpression)
    {
      if (memberExpression.Expression.NodeType != ExpressionType.Parameter)
        return PreviousClause.ResolveField (databaseInfo, memberExpression, fullFieldExpression);
      else
      {
        ParameterExpression parameterExpression = (ParameterExpression) (memberExpression.Expression);
        if (parameterExpression.Name != Identifier.Name)
          return PreviousClause.ResolveField (databaseInfo, memberExpression, fullFieldExpression);

        CheckExpressionNameAndType (parameterExpression.Name, parameterExpression.Type, fullFieldExpression);
        return CreateFieldDescriptor (memberExpression.Member, databaseInfo, fullFieldExpression);
      }
    }

    private FieldDescriptor ResolveParameter (ParameterExpression parameterExpression, IDatabaseInfo databaseInfo, Expression fullFieldExpression)
    {
      if (parameterExpression.Name != Identifier.Name)
        return PreviousClause.ResolveField (databaseInfo, parameterExpression, fullFieldExpression);

      CheckExpressionNameAndType (parameterExpression.Name, parameterExpression.Type, fullFieldExpression);
      return CreateFieldDescriptor (null, databaseInfo, fullFieldExpression);
    }
  }
}