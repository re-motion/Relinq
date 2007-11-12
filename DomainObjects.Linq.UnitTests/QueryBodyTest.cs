using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using OrderDirection=Rubicon.Data.DomainObjects.Linq.OrderDirection;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class QueryBodyTest
  {
    [Test]
    public void InitializeWithISelectOrGroupClause()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      ISelectGroupClause iSelectOrGroupClause = new SelectClause (expression);

      QueryBody queryBody = new QueryBody (iSelectOrGroupClause);

      Assert.AreSame(iSelectOrGroupClause,queryBody.ISelectOrGroupClause);
    }

    [Test]
    public void InitializeWithISelectOrGroupClauseAndOrderByClause()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      ISelectGroupClause iSelectOrGroupClause = new SelectClause (expression);
      
      OrderingClause ordering = new OrderingClause (expression, OrderDirection.Asc);

      OrderByClause orderByClause = new OrderByClause (ordering);
      

      QueryBody queryBody = new QueryBody (iSelectOrGroupClause, orderByClause);

      Assert.AreSame (iSelectOrGroupClause, queryBody.ISelectOrGroupClause);
      Assert.AreSame (orderByClause, queryBody.OrderByClause);

    }

    [Test]
    public void AddIFromLetWhereClause()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      ISelectGroupClause iSelectOrGroupClause = new SelectClause (expression);

      QueryBody queryBody = new QueryBody (iSelectOrGroupClause);

      IFromLetWhereClause iFromLetWhereCLause = new WhereClause (expression);

      queryBody.Add (iFromLetWhereCLause);

      Assert.AreEqual (1, queryBody.FromLetWhereCount);
      Assert.That (queryBody.FromLetWhere, List.Contains (iFromLetWhereCLause));
    }

    [Test]
    public void QueryBody_ImplementsIQueryElement()
    {
      QueryBody queryBody = CreateQueryBody();
      Assert.IsInstanceOfType (typeof (IQueryElement), queryBody);
    }

    [Test]
    public void Accept()
    {
      QueryBody queryBody = CreateQueryBody ();
      MockRepository repository = new MockRepository();

      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();

      visitorMock.VisitQueryBody (queryBody);
      
      repository.ReplayAll();
      queryBody.Accept (visitorMock);
      repository.VerifyAll();
    }

    public QueryBody CreateQueryBody()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      ISelectGroupClause iSelectOrGroupClause = new SelectClause (expression);

      return new QueryBody (iSelectOrGroupClause);

    }
  }
}