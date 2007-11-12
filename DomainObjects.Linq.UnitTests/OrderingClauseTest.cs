using System;
using System.Linq.Expressions;
using NUnit.Framework;
using OrderDirection=Rubicon.Data.DomainObjects.Linq.OrderDirection;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class OrderingClauseTest
  {
   
    [Test]
    public void InitializeWithExpressionAndOrderDirectionAsc()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      OrderDirection directionAsc  = OrderDirection.Asc;
      

      OrderingClause ordering = new OrderingClause(expression,directionAsc);
      

      Assert.AreSame (expression, ordering.Expression);
      Assert.AreEqual (directionAsc, ordering.OrderDirection);
    }

    [Test]
    public void InitializeWithExpressionAndOrderDirectionDesc ()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      OrderDirection directionAsc = OrderDirection.Desc;


      OrderingClause ordering = new OrderingClause (expression, directionAsc);


      Assert.AreSame (expression, ordering.Expression);
      Assert.AreEqual (directionAsc, ordering.OrderDirection);
    }
  }
}