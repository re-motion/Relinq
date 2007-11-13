using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Clauses
{
  public class FromClause : IFromLetWhereClause
  {
    private readonly ParameterExpression _identifier;
    private readonly Expression _expression;
    private readonly List<JoinClause> _joinClauses = new List<JoinClause>();

    public FromClause (ParameterExpression id, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("id", id);
      ArgumentUtility.CheckNotNull ("expression", expression);

      _identifier = id;
      _expression = expression;
    }

    public ParameterExpression Identifier
    {
      get { return _identifier; }
    }

    public Expression Expression
    {
      get { return _expression; }
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

    public void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitFromClause (this);
    }


  }
}