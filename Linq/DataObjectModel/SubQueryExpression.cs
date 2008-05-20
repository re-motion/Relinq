using System.Linq.Expressions;

namespace Remotion.Data.Linq.DataObjectModel
{
#warning TODO: Move to outer namespace
  public class SubQueryExpression : Expression
  {
    public QueryModel QueryModel { get; private set; }

    public SubQueryExpression (QueryModel queryModel)
      : base ((ExpressionType) int.MaxValue, queryModel.ResultType)
    {
      QueryModel = queryModel;
    }
  }
}