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

    public static JoinClause CreateJoinClause ()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression inExpression = ExpressionHelper.CreateExpression ();
      Expression onExpression = ExpressionHelper.CreateExpression ();
      Expression equalityExpression = ExpressionHelper.CreateExpression ();

      return new JoinClause (identifier, inExpression, onExpression, equalityExpression);
    }

    public static FromClause CreateFromClause ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      Expression expression = ExpressionHelper.CreateExpression ();
      
      return new FromClause (id, expression);
    }



    public static GroupClause CreateGroupClause ()
    {
      Expression groupExpression = ExpressionHelper.CreateExpression ();
      Expression byExpression = ExpressionHelper.CreateExpression ();

      return new GroupClause (groupExpression, byExpression);
    }


    public static LetClause CreateLetClause ()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression expression = ExpressionHelper.CreateExpression ();

      return new LetClause (identifier, expression);
    }

    public static OrderingClause CreateOrderingClause()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      return new OrderingClause (expression, OrderDirection.Asc);
    }

    public static OrderByClause CreateOrderByClause()
    {
      OrderingClause ordering = ExpressionHelper.CreateOrderingClause ();
      return new OrderByClause (ordering);
    }

    public static QueryBody CreateQueryBody()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      ISelectGroupClause iSelectOrGroupClause = new SelectClause (expression);

      return new QueryBody (iSelectOrGroupClause);

    }

    public static SelectClause CreateSelectClause ()
    {
      Expression expression = ExpressionHelper.CreateExpression ();

      return new SelectClause (expression);
    }

    public static WhereClause CreateWhereClause ()
    {
      Expression boolExpression = ExpressionHelper.CreateExpression ();
      return new WhereClause (boolExpression);
    }
  }
}