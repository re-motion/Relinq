using System;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class LetClause : IFromLetWhereClause
  {
    private readonly ParameterExpression _identifier;
    private readonly Expression _expression;

    public LetClause (ParameterExpression identifier, Expression expression)
    {
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("expression", expression);

      _identifier = identifier;
      _expression = expression;
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    public ParameterExpression Identifier
    {
      get { return _identifier; }
    }
  }
}