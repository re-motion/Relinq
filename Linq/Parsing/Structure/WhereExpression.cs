using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing.Structure;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class WhereExpression : BodyExpressionBase<LambdaExpression>
  {
    public WhereExpression (LambdaExpression expression)
        : base (expression)
    {
    }
  }
}