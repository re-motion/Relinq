using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class FromExpressionData : BodyExpressionDataBase<Expression>
  {
    public ParameterExpression Identifier { get; private set; }

    public FromExpressionData (Expression expression,ParameterExpression identifier)
        : base (expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("identifier", identifier);

      Identifier = identifier;
    }
  }
}