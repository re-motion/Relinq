using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;


namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class OrderByClauseTest
  {

    [Test]
    public void InitializeWithOneOrdering()
    {
      OrderingClause ordering = ExpressionHelper.CreateOrderingClause();
      OrderByClause orderBy = new OrderByClause (ordering);

      Assert.AreEqual (1, orderBy.OrderByClauseCount);
      
    }
    
    [Test]
    public void AddMoreOrderings()
    {
      OrderingClause ordering1 = ExpressionHelper.CreateOrderingClause ();
      OrderingClause ordering2 = ExpressionHelper.CreateOrderingClause ();
      OrderByClause orderBy = new OrderByClause (ordering1);
      orderBy.Add (ordering2);

      Assert.That (orderBy.OrderingList, Is.EqualTo (new object[] { ordering1, ordering2 }));
      Assert.AreEqual (2, orderBy.OrderByClauseCount);

    }

    [Test]
    public void OrderByClause_ImplementsIQueryElement()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), orderByClause);
    }

    [Test]
    public void Accept()
    {
      OrderByClause orderByClause = ExpressionHelper.CreateOrderByClause ();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();

      visitorMock.VisitOrderByClause(orderByClause);

      repository.ReplayAll();

      orderByClause.Accept (visitorMock);

      repository.VerifyAll();

    }
  }
}