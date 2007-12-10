using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
{
  public abstract class FromLetWhereExpressionBase<TExpression> : FromLetWhereExpressionBase
      where TExpression : Expression
  {
    public TExpression Expression { get; private set; }

    public FromLetWhereExpressionBase(TExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression = expression;
    }
  }

  public abstract class FromLetWhereExpressionBase
  {
  }
}