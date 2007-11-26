using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Clauses
{
  public class MainFromClause : FromClauseBase
  {

    public MainFromClause (ParameterExpression identifier, IQueryable querySource): base(identifier)
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
  }
}