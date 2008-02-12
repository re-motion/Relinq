using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Visitor;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq
{
  public class QueryExpression : IQueryElement
  {
    private readonly MainFromClause _fromClause;
    private readonly QueryBody _queryBody;
    private Expression _expressionTree;

    
    public QueryExpression (MainFromClause fromClause, QueryBody queryBody, Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryBody", queryBody);

      _fromClause = fromClause;
      _queryBody = queryBody;
      _expressionTree = expressionTree;

    }

    public QueryExpression (MainFromClause fromClause, QueryBody queryBody) : this (fromClause,queryBody,null)
    {
    }

    public MainFromClause FromClause
    {
      get { return _fromClause; }
    }

    public QueryBody QueryBody
    {
      get { return _queryBody; }
    }

    public FromClauseBase GetFromClause(string identifierName, Type identifierType)
    {
      ArgumentUtility.CheckNotNull ("identifierName", identifierName);
      ArgumentUtility.CheckNotNull ("identifierType", identifierType);

      if (identifierName == FromClause.Identifier.Name)
      {
        FromClause.CheckIdentifierType (identifierType);
        return FromClause;
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

    public FieldDescriptor ResolveField (IDatabaseInfo databaseInfo, Expression fieldAccessExpression)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);

      QueryExpressionResolveVisitor visitor = new QueryExpressionResolveVisitor (this);
      QueryExpressionResolveVisitor.Result visitorResult = visitor.ParseAndReduce (fieldAccessExpression);

      return visitorResult.FromClause.ResolveField (databaseInfo, visitorResult.ReducedExpression, fieldAccessExpression);
    }
  }
}
