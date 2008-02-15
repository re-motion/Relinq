using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Data.Linq.Visitor;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq
{
  public class QueryExpression : IQueryElement
  {
    private Expression _expressionTree;


    public QueryExpression (MainFromClause mainFromClause, QueryBody queryBody, Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("mainFromClause", mainFromClause);
      ArgumentUtility.CheckNotNull ("queryBody", queryBody);

      MainFromClause = mainFromClause;
      QueryBody = queryBody;
      _expressionTree = expressionTree;
    }

    public QueryExpression (MainFromClause fromClause, QueryBody queryBody)
        : this (fromClause, queryBody, null)
    {
    }

    public MainFromClause MainFromClause { get; private set; }
    public QueryBody QueryBody { get; private set; }

    public FromClauseBase GetFromClause (string identifierName, Type identifierType)
    {
      ArgumentUtility.CheckNotNull ("identifierName", identifierName);
      ArgumentUtility.CheckNotNull ("identifierType", identifierType);

      if (identifierName == MainFromClause.Identifier.Name)
      {
        MainFromClause.CheckResolvedIdentifierType (identifierType);
        return MainFromClause;
      }
      else
        return QueryBody.GetFromClause (identifierName, identifierType);
    }

    public void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      visitor.VisitQueryExpression (this);
    }

    public override string ToString ()
    {
      StringVisitor sv = new StringVisitor();
      sv.VisitQueryExpression (this);
      return sv.ToString();
    }

    public Expression GetExpressionTree ()
    {
      if (_expressionTree == null)
      {
        ExpressionTreeBuildingVisitor visitor = new ExpressionTreeBuildingVisitor();
        Accept (visitor);
        _expressionTree = visitor.ExpressionTree;
      }
      return _expressionTree;
    }

    public FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, JoinedTableContext context, Expression fieldAccessExpression)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);

      return new QueryExpressionFieldResolver (this).ResolveField (databaseInfo, context, fieldAccessExpression);
    }
  }
}