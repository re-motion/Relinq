using System;
using Rubicon.Utilities;
using System.Linq.Expressions;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class SelectClause : ISelectGroupClause,IQueryElement
  {
    private readonly Expression _expression;

    public SelectClause (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _expression = expression;
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    public void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitSelectClause (this);
    }
  }
}