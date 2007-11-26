using System;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Clauses
{
  public class AdditionalFromClause : FromClauseBase,IFromLetWhereClause
  {
    

    public AdditionalFromClause (ParameterExpression identifier, Expression expression) : base(identifier)
    {
      ArgumentUtility.CheckNotNull ("identifier", identifier);
      ArgumentUtility.CheckNotNull ("expression", expression);

      Expression = expression;
    }
    public Expression Expression { get; private set; }

    public override void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitAdditionalFromClause (this);
    }
  }
}