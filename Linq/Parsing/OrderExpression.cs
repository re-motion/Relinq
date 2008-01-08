using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
{
  public class OrderExpression : BodyExpressionBase<Expression>
  {
    public OrderExpression (bool firstOrderBy, OrderDirection orderDirection, LambdaExpression expression)
      : base (expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      FirstOrderBy = firstOrderBy;
      OrderDirection = orderDirection;
      Expression = expression;
    }

    public bool FirstOrderBy { get; private set; }
    public OrderDirection OrderDirection { get; private set; }
    public LambdaExpression Expression { get; private set; }

    public override string ToString ()
    {
      if (FirstOrderBy)
        return string.Format ("orderby {0} {1}", Expression, OrderDirection);
      else
        return string.Format ("thenby {0} {1}", Expression, OrderDirection);
    }
  }
}