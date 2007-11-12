using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
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
      OrderDirection directionAsc = OrderDirection.Asc;


      OrderingClause ordering = new OrderingClause (expression, directionAsc);


      Assert.AreSame (expression, ordering.Expression);
      Assert.AreEqual (directionAsc, ordering.OrderDirection);
    }

    [Test]
    public void OrderingClause_ImplementsIQueryElement()
    {
      OrderingClause orderingClause = CreateOrderingClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), orderingClause);
    }

    [Test]
    public void Accept()
    {
      OrderingClause orderingClause = CreateOrderingClause ();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();

      visitorMock.VisitOrderingClause (orderingClause);

      repository.ReplayAll();

      orderingClause.Accept (visitorMock);

      repository.VerifyAll();
    }
    

    public OrderingClause CreateOrderingClause()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      OrderDirection directionAsc = OrderDirection.Asc;
      return new OrderingClause (expression, directionAsc);
    }



  }
}