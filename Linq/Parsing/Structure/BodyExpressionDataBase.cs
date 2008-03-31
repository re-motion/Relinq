using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public abstract class BodyExpressionDataBase<TExpression> : BodyExpressionDataBase
      where TExpression : Expression
  {
    public TExpression Expression { get; private set; }

    public BodyExpressionDataBase(TExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Expression = expression;
    }
  }

  public abstract class BodyExpressionDataBase
  {
  }
}