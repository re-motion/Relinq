using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
  }
}