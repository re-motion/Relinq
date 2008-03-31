using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing.Structure;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class WhereExpressionData : BodyExpressionDataBase<LambdaExpression>
  {
    public WhereExpressionData (LambdaExpression expression)
        : base (expression)
    {
    }
  }
}