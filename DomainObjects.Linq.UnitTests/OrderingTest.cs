using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class OrderingTest
  {
   
    [Test]
    public void InitializeWithExpressionAndOrderDirectionAsc()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      Ordering.OrderDirection directionAsc  = Ordering.OrderDirection.Asc;
      

      Ordering ordering = new Ordering(expression,directionAsc);
      

      Assert.AreSame (expression, ordering.Expression);
      Assert.AreEqual (directionAsc, Ordering.OrderDirection.Asc);
    }

    [Test]
    public void InitializeWithExpressionAndOrderDirectionDesc ()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      Ordering.OrderDirection directionAsc = Ordering.OrderDirection.Desc;


      Ordering ordering = new Ordering (expression, directionAsc);


      Assert.AreSame (expression, ordering.Expression);
      Assert.AreEqual (directionAsc, Ordering.OrderDirection.Desc);
    }
  }
}