using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;


namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class OrderByClauseTest
  {
    [Test]
    public void InitializeWithOneOrdering()
    {
      OrderingClause ordering = ExpressionHelper.CreateOrderingClause();
      OrderByClause orderBy = new OrderByClause (ordering);

      Assert.AreEqual (1, orderBy.OrderingList.Count);
      Assert.IsNotNull (orderBy.PreviousClause);
      Assert.AreSame (orderBy.OrderingList[0].PreviousClause, orderBy.PreviousClause);
    }
    
    [Test]
    public void AddMoreOrderings()
    {
      OrderingClause ordering1 = ExpressionHelper.CreateOrderingClause ();
      OrderingClause ordering2 = ExpressionHelper.CreateOrderingClause ();
      OrderByClause orderBy = new OrderByClause (ordering1);
      orderBy.Add (ordering2);

      Assert.That (orderBy.OrderingList, Is.EqualTo (new object[] { ordering1, ordering2 }));
      Assert.AreEqual (2, orderBy.OrderingList.Count);

      Assert.IsNotNull (orderBy.PreviousClause);
      Assert.AreSame (orderBy.OrderingList[0].PreviousClause, orderBy.PreviousClause);
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

    [Test]
    public void Resolve()
    {
      Expression resolvedFieldExpression = ExpressionHelper.CreateExpression();
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      MockRepository repository = new MockRepository();
      IClause previousClause = repository.CreateMock<IClause>();

      OrderingClause ordering = new OrderingClause (previousClause, expression, OrderDirection.Asc);
      OrderByClause clause = new OrderByClause (ordering);

      Table table = new Table ("Foo", "foo");
      FieldDescriptor fieldDescriptor = new FieldDescriptor (null, ExpressionHelper.CreateMainFromClause (), table, new Column (table, "Bar"));
      Expect.Call (previousClause.ResolveField (StubDatabaseInfo.Instance, resolvedFieldExpression, resolvedFieldExpression)).Return (fieldDescriptor);

      repository.ReplayAll();

      FieldDescriptor resolvedFieldDescriptor = clause.ResolveField (StubDatabaseInfo.Instance, resolvedFieldExpression, resolvedFieldExpression);
      Assert.AreEqual (fieldDescriptor, resolvedFieldDescriptor);
      repository.VerifyAll();
    }
  }
}