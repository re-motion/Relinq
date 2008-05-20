using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class OrderExpressionData : BodyExpressionDataBase<LambdaExpression>
  {
    public OrderExpressionData (bool firstOrderBy, OrderDirection orderDirection, LambdaExpression expression)
        : base (expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      FirstOrderBy = firstOrderBy;
      OrderDirection = orderDirection;
    }

    public bool FirstOrderBy { get; private set; }
    public OrderDirection OrderDirection { get; private set; }

    public override string ToString ()
    {
      if (FirstOrderBy)
        return string.Format ("orderby {0} {1}", Expression, OrderDirection);
      else
        return string.Format ("thenby {0} {1}", Expression, OrderDirection);
    }
  }
}