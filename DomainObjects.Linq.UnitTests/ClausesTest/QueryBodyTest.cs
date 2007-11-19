using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using OrderDirection=Rubicon.Data.DomainObjects.Linq.Clauses.OrderDirection;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ClausesTest
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

      Assert.AreSame(iSelectOrGroupClause,queryBody.SelectOrGroupClause);
    }

    [Test]
    public void InitializeWithISelectOrGroupClauseAndOrderByClause()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      ISelectGroupClause iSelectOrGroupClause = new SelectClause (expression);
      
      OrderingClause ordering = new OrderingClause (expression, OrderDirection.Asc);

      OrderByClause orderByClause = new OrderByClause (ordering);
      

      QueryBody queryBody = new QueryBody (iSelectOrGroupClause, orderByClause);

      Assert.AreSame (iSelectOrGroupClause, queryBody.SelectOrGroupClause);
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

      Assert.AreEqual (1, queryBody.FromLetWhereClauseCount);
      Assert.That (queryBody.FromLetWhereClauses, List.Contains (iFromLetWhereCLause));
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