using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class LetClause : IBodyClause
  {
    private readonly ParameterExpression _identifier;
    private readonly Expression _expression;

    public LetClause (IClause previousClause,ParameterExpression identifier, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      _identifier = identifier;
      _expression = expression;
      PreviousClause = previousClause;
    }

    public IClause PreviousClause { get; private set; }

    public Expression Expression
    {
      get { return _expression; }
    }

    public ParameterExpression Identifier
    {
      get { return _identifier; }
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitLetClause (this);
    }
  }
}