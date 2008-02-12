using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using OrderDirection=Rubicon.Data.Linq.Clauses.OrderDirection;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class OrderingClauseTest
  {
   
    [Test]
    public void InitializeWithExpressionAndOrderDirectionAsc()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression();
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
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
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