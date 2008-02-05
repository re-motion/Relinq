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

    [Test]
    public void Resolve ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      MockRepository repository = new MockRepository ();
      IClause previousClause = repository.CreateMock<IClause> ();

      OrderingClause clause = new OrderingClause (previousClause, expression, OrderDirection.Asc);

      Expression resolvedFieldExpression = ExpressionHelper.CreateExpression ();
      Table table = new Table ("Foo", "foo");
      FieldDescriptor fieldDescriptor = new FieldDescriptor (null, ExpressionHelper.CreateMainFromClause (), table, new Column (table, "Bar"));
      Expect.Call (previousClause.ResolveField (StubDatabaseInfo.Instance, resolvedFieldExpression, resolvedFieldExpression)).Return (fieldDescriptor);

      repository.ReplayAll ();

      FieldDescriptor resolvedFieldDescriptor = clause.ResolveField (StubDatabaseInfo.Instance, resolvedFieldExpression, resolvedFieldExpression);
      Assert.AreEqual (fieldDescriptor, resolvedFieldDescriptor);
      repository.VerifyAll ();
    }
  }
}