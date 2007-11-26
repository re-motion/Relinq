using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Clauses
{
  public abstract class FromClauseBase : IQueryElement
  {
    private readonly ParameterExpression _identifier;
    private readonly List<JoinClause> _joinClauses = new List<JoinClause>();

    public FromClauseBase (ParameterExpression identifier)
    {
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      _identifier = identifier;
    }

    public ParameterExpression Identifier
    {
      get { return _identifier; }
    }

    public IEnumerable<JoinClause> JoinClauses
    {
      get { return _joinClauses; }
    }

    public int JoinClauseCount
    {
      get { return _joinClauses.Count; }
    }

    public void Add (JoinClause joinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      _joinClauses.Add (joinClause);
    }


    public abstract void Accept (IQueryVisitor visitor);
  }
}