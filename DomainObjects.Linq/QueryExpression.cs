using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.Visitor;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class QueryExpression : IQueryElement
  {
    private readonly MainFromClause _fromClause;
    private readonly QueryBody _queryBody;

    public QueryExpression (MainFromClause from, QueryBody queryBody)
    {
      ArgumentUtility.CheckNotNull ("from", from);
      ArgumentUtility.CheckNotNull ("queryBody", queryBody);

      _fromClause = from;
      _queryBody = queryBody;
    }

    public MainFromClause FromClause
    {
      get { return _fromClause; }
    }

    public QueryBody QueryBody
    {
      get { return _queryBody; }
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

    public Expression BuildExpressionTree ()
    {
      ExpressionTreeBuildingVisitor visitor = new ExpressionTreeBuildingVisitor ();
      Accept (visitor);
      return visitor.ExpressionTree;
    }
  }
}
