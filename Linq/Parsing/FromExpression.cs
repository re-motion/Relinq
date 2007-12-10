using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
{
  public class FromExpression : FromLetWhereExpressionBase<Expression>
  {
    public ParameterExpression Identifier { get; private set; }

    public FromExpression (Expression expression,ParameterExpression identifier)
        : base (expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("identifier", identifier);

      Identifier = identifier;

    }
  }
}