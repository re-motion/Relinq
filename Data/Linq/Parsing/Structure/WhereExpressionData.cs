using System.Linq.Expressions;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class WhereExpressionData : BodyExpressionDataBase<LambdaExpression>
  {
    public WhereExpressionData (LambdaExpression expression)
        : base (expression)
    {
    }
  }
}