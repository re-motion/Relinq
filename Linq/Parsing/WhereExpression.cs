using System.Linq.Expressions;

namespace Rubicon.Data.Linq.Parsing
{
  public class WhereExpression : BodyExpressionBase<LambdaExpression>
  {
    public WhereExpression (LambdaExpression expression)
        : base (expression)
    {
    }
  }
}