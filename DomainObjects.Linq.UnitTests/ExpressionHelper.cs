using System.Linq.Expressions;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  public static class ExpressionHelper
  {
    public static Expression CreateExpression ()
    {
      return Expression.NewArrayInit (typeof (int));
    }

    public static ParameterExpression CreateParameterExpression ()
    {
      return Expression.Parameter (typeof (int), "i");
    }
  }
}