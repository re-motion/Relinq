using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public abstract class BodyExpressionBase<TExpression> : BodyExpressionBase
      where TExpression : Expression
  {
    public TExpression Expression { get; private set; }

    public BodyExpressionBase(TExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression = expression;
    }
  }

  public abstract class BodyExpressionBase
  {
  }
}