using System.Linq.Expressions;

namespace Remotion.Data.Linq.Expressions
{
  public class QueryModelExpressionBase : Expression
  {
    public QueryModelExpressionBase (QueryModel queryModel)
        : base ((ExpressionType) int.MaxValue, queryModel.ResultType)
    {
      QueryModel = queryModel;
    }

    public QueryModel QueryModel { get; private set; }
  }
}