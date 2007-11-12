using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;


namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class OrderByClauseTest
  {

    [Test]
    public void InitializeWithOneOrdering()
    {
      OrderingClause ordering = CreateOrderingClause();
      OrderByClause orderBy = new OrderByClause (ordering);

      Assert.AreEqual (1, orderBy.OrderByClauseCount);
      
    }
    
    [Test]
    public void AddMoreOrderings()
    {
      OrderingClause ordering1 = CreateOrderingClause ();
      OrderingClause ordering2 = CreateOrderingClause ();
      OrderByClause orderBy = new OrderByClause (ordering1);
      orderBy.Add (ordering2);

      Assert.That (orderBy.OrderList, Is.EqualTo (new object[] { ordering1, ordering2 }));
      Assert.AreEqual (2, orderBy.OrderByClauseCount);

    }

    [Test]
    public void OrderByClause_ImplementsIQueryElement()
    {
      OrderByClause orderByClause = CreateOrderByClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), orderByClause);
    }

    [Test]
    public void Accept()
    {
      OrderByClause orderByClause = CreateOrderByClause ();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();

      visitorMock.VisitOrderByClause(orderByClause);

      repository.ReplayAll();

      orderByClause.Accept (visitorMock);

      repository.VerifyAll();

    }
    
    public OrderingClause CreateOrderingClause()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      return new OrderingClause (expression, OrderDirection.Asc);
    }

    public OrderByClause CreateOrderByClause()
    {
      OrderingClause ordering = CreateOrderingClause ();
      return new OrderByClause (ordering);
    }
  }
}