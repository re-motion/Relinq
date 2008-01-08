using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using OrderDirection=Rubicon.Data.Linq.Clauses.OrderDirection;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class QueryBodyTest
  {
    [Test]
    public void InitializeWithISelectOrGroupClause()
    {
      ISelectGroupClause iSelectOrGroupClause = ExpressionHelper.CreateSelectClause();

      QueryBody queryBody = new QueryBody (iSelectOrGroupClause);

      Assert.AreSame(iSelectOrGroupClause,queryBody.SelectOrGroupClause);
    }

    [Test]
    public void InitializeWithISelectOrGroupClauseAndOrderByClause()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      ISelectGroupClause iSelectOrGroupClause = ExpressionHelper.CreateSelectClause ();
      
      OrderingClause ordering = new OrderingClause (ExpressionHelper.CreateClause(),expression, OrderDirection.Asc);

      
      QueryBody queryBody = new QueryBody (iSelectOrGroupClause);
      OrderByClause orderByClause = new OrderByClause (ordering);

      queryBody.Add (orderByClause);

      Assert.AreSame (iSelectOrGroupClause, queryBody.SelectOrGroupClause);

      Assert.AreEqual (1, queryBody.BodyClauseCount);
      Assert.That (queryBody.BodyClauses, List.Contains (orderByClause));
    }

    [Test]
    public void AddSeveralOrderByClauses()
    {
      ISelectGroupClause iSelectOrGroupClause = ExpressionHelper.CreateSelectClause ();
      QueryBody queryBody = new QueryBody (iSelectOrGroupClause);

      IBodyClause orderByClause1 = ExpressionHelper.CreateOrderByClause();
      IBodyClause orderByClause2 = ExpressionHelper.CreateOrderByClause ();

      queryBody.Add (orderByClause1);
      queryBody.Add (orderByClause2);

      Assert.AreEqual (2, queryBody.BodyClauseCount);
      Assert.That (queryBody.BodyClauses, Is.EqualTo(new object[] {orderByClause1,orderByClause2}));
    }

    
    [Test]
    public void AddBodyClause()
    {
      ISelectGroupClause iSelectOrGroupClause = ExpressionHelper.CreateSelectClause ();
      QueryBody queryBody = new QueryBody (iSelectOrGroupClause);

      IBodyClause clause = ExpressionHelper.CreateWhereClause();
      queryBody.Add (clause);

      Assert.AreEqual (1, queryBody.BodyClauseCount);
      Assert.That (queryBody.BodyClauses, List.Contains (clause));
    }

    [Test]
    public void QueryBody_ImplementsIQueryElement()
    {
      QueryBody queryBody = ExpressionHelper.CreateQueryBody();
      Assert.IsInstanceOfType (typeof (IQueryElement), queryBody);
    }

    [Test]
    public void Accept()
    {
      QueryBody queryBody = ExpressionHelper.CreateQueryBody ();
      MockRepository repository = new MockRepository();

      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();

      visitorMock.VisitQueryBody (queryBody);
      
      repository.ReplayAll();
      queryBody.Accept (visitorMock);
      repository.VerifyAll();
    }
  }
}