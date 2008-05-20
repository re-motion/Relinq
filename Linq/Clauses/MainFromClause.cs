using System;
using System.Linq.Expressions;
using Remotion.Utilities;


namespace Remotion.Data.Linq.Clauses
{
  public class MainFromClause : FromClauseBase
  {
    public MainFromClause (ParameterExpression identifier, Expression querySource): base(null,identifier)
    {
      ArgumentUtility.CheckNotNull ("querySource", querySource);
      QuerySource = querySource;
    }

    public Expression QuerySource { get; private set; }

    public override void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitMainFromClause (this);
    }

    public override Type GetQuerySourceType ()
    {
      return QuerySource.Type;
    }
  }
}