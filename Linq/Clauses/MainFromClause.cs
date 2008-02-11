using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class MainFromClause : FromClauseBase
  {

    public MainFromClause (ParameterExpression identifier, IQueryable querySource): base(null,identifier)
    {
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

      switch (partialFieldExpression.NodeType)
      {
        case ExpressionType.MemberAccess:
          return ResolveMemberAccess (databaseInfo, (MemberExpression) partialFieldExpression, fullFieldExpression);
        case ExpressionType.Parameter:
          return ResolveParameter (databaseInfo, (ParameterExpression) partialFieldExpression, fullFieldExpression);
        default:
          throw ParserUtility.CreateParserException ("MemberExpression or ParameterExpression", partialFieldExpression, "resolving field access",
              fullFieldExpression);
      }
    }

    private FieldDescriptor ResolveMemberAccess (IDatabaseInfo databaseInfo, MemberExpression memberExpression, Expression fullFieldExpression)
    {
      ParameterExpression parameterExpression = ParserUtility.GetTypedExpression<ParameterExpression> (memberExpression.Expression,
          "resolving field access", fullFieldExpression);

      CheckExpressionNameAndType (parameterExpression.Name, parameterExpression.Type, fullFieldExpression);
      return CreateFieldDescriptor(memberExpression.Member, databaseInfo, fullFieldExpression);
    }

    private FieldDescriptor ResolveParameter (IDatabaseInfo databaseInfo, ParameterExpression parameterExpression, Expression fullFieldExpression)
    {
      CheckExpressionNameAndType (parameterExpression.Name, parameterExpression.Type, fullFieldExpression);
      return CreateFieldDescriptor (null, databaseInfo, fullFieldExpression);
    }

    private void CheckExpressionNameAndType (string name, Type type, Expression fullFieldExpression)
    {
      if (name != Identifier.Name)
      {
        string message = string.Format ("There is no from clause defining identifier '{0}', which is used in expression '{1}'.",
            name, fullFieldExpression);
        throw new ParserException (message);
      }

      if (type != Identifier.Type)
      {
        string message = string.Format ("The from identifier '{0}' has a different type ({1}) than expected ({2}) in expression '{3}'.",
            name, Identifier.Type, type, fullFieldExpression);
        throw new ParserException (message);
      }
    }

  }
}