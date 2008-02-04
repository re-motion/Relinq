using System;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class AdditionalFromClause : FromClauseBase,IBodyClause
  {
    public AdditionalFromClause (IClause previousClause, ParameterExpression identifier,
        LambdaExpression fromExpression, LambdaExpression projectionExpression)
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

      MemberExpression transparentMemberExpression = ParserUtility.GetTypedExpression<MemberExpression> (memberExpression.Expression,
          "resolving field access", fullFieldExpression);

      if (transparentMemberExpression.Member.Name != Identifier.Name)
      {
        // remove transparent identifier added by the SelectMany call corresponding to this clause
        Expression reducedExpression = new MemberAccessReducingTreeVisitor().ReduceInnermostMemberExpression (memberExpression);
        return PreviousClause.ResolveField (databaseInfo, reducedExpression, fullFieldExpression);
      }

      if (transparentMemberExpression.Type != Identifier.Type)
      {
        string message = string.Format ("The from identifier '{0}' has a different type ({1}) than expected in expression '{2}' ({3}).",
            Identifier.Name, Identifier.Type, fullFieldExpression, transparentMemberExpression.Type);
        throw new ParserException (message);
      }

      try
      {
        Table table = DatabaseInfoUtility.GetTableForFromClause (databaseInfo, this);
        Column column = DatabaseInfoUtility.GetColumn (databaseInfo, table, memberExpression.Member);
        return new FieldDescriptor (column, this);
      }
      catch (Exception ex)
      {
        string message = string.Format ("Could not retrieve database metadata for field expression '{0}'.", fullFieldExpression);
        throw new ParserException (message, ex);
      }
    }
  }
}