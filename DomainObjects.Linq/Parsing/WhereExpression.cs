using System.Linq.Expressions;

namespace Rubicon.Data.DomainObjects.Linq.Parsing
{
  public class WhereExpression : FromLetWhereExpressionBase<LambdaExpression>
  {
    public WhereExpression (LambdaExpression expression)
        : base (expression)
    {
    }
  }
}