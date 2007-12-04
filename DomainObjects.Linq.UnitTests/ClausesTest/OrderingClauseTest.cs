using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using OrderDirection=Rubicon.Data.DomainObjects.Linq.Clauses.OrderDirection;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class OrderingClauseTest
  {
   
    [Test]
    public void InitializeWithExpressionAndOrderDirectionAsc()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      OrderDirection directionAsc  = OrderDirection.Asc;

      IClause clause = ExpressionHelper.CreateClause();
      
      OrderingClause ordering = new OrderingClause(clause, expression,directionAsc);


      Assert.AreSame (clause, ordering.PreviousClause);
      Assert.AreSame (expression, ordering.Expression);
      Assert.AreEqual (directionAsc, ordering.OrderDirection);
    }

    [Test]
    public void InitializeWithExpressionAndOrderDirectionDesc ()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      OrderDirection directionAsc = OrderDirection.Asc;

      IClause clause = ExpressionHelper.CreateClause ();

      OrderingClause ordering = new OrderingClause (clause,expression, directionAsc);

      Assert.AreSame (clause, ordering.PreviousClause);
      Assert.AreSame (expression, ordering.Expression);
      Assert.AreEqual (directionAsc, ordering.OrderDirection);
    }

    [Test]
    public void OrderingClause_ImplementsIQueryElement()
    {
      OrderingClause orderingClause = ExpressionHelper.CreateOrderingClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), orderingClause);
    }

    [Test]
    public void Accept()
    {
      OrderingClause orderingClause = ExpressionHelper.CreateOrderingClause ();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();

      visitorMock.VisitOrderingClause (orderingClause);

      repository.ReplayAll();

      orderingClause.Accept (visitorMock);

      repository.VerifyAll();
    }
    

    



  }
}