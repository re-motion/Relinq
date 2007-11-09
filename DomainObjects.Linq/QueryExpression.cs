using System;
using System.Collections.Generic;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class QueryExpression
  {
    private readonly FromClause _fromClause;
    private readonly QueryBody _queryBody;

    public QueryExpression (FromClause from, QueryBody queryBody)
    {
      ArgumentUtility.CheckNotNull ("from", from);
      ArgumentUtility.CheckNotNull ("queryBody", queryBody);

      _fromClause = from;
      _queryBody = queryBody;
    }

    public FromClause FromClause
    {
      get { return _fromClause; }
    }

    public QueryBody QueryBody
    {
      get { return _queryBody; }
    }
  }
}
