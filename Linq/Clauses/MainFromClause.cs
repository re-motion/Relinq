using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class MainFromClause : FromClauseBase
  {

    public MainFromClause (ParameterExpression identifier, IQueryable querySource): base(null,identifier)
    {
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("querySource", querySource);

      QuerySource = querySource;
    }

    public IQueryable QuerySource { get; private set; }

    public override void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitMainFromClause (this);
    }

    public override Type GetQuerySourceType ()
    {
      return QuerySource.GetType();
    }

    public override FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, Expression partialFieldExpression, Expression fullFieldExpression)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);

      MemberExpression memberExpression =
          ParserUtility.GetTypedExpression<MemberExpression> (partialFieldExpression, "resolving field access", fullFieldExpression);
      ParameterExpression parameterExpression = ParserUtility.GetTypedExpression<ParameterExpression> (memberExpression.Expression,
          "resolving field access", fullFieldExpression);
      
      if (parameterExpression.Name != Identifier.Name)
      {
        string message = string.Format ("There is no from clause defining identifier '{0}', which is used in expression '{1}'.",
            parameterExpression.Name, fullFieldExpression);
        throw new ParserException (message);
      }

      if (parameterExpression.Type != Identifier.Type)
      {
        string message = string.Format ("The from identifier '{0}' has a different type ({1}) than expected in expression '{2}' ({3}).",
            parameterExpression.Name, Identifier.Type, fullFieldExpression, parameterExpression.Type);
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