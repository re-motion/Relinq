using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
{
  public class OrderExpression
  {
    public OrderExpression (bool firstOrderBy, OrderDirection orderDirection, UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      FirstOrderBy = firstOrderBy;
      OrderDirection = orderDirection;
      Expression = expression;
    }

    public bool FirstOrderBy { get; private set; }
    public OrderDirection OrderDirection { get; private set; }
    public UnaryExpression Expression { get; private set; }

  }
}