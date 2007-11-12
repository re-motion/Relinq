using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class QueryExpressionTest
  {
    [Test]
    public void Initialize_WithFromClauseAndBody ()
    {
      FromClause fromClause = CreateFromClause();
      QueryBody queryBody = CreateQueryBody();
      QueryExpression model = new QueryExpression (fromClause, queryBody);
      Assert.AreSame (fromClause, model.FromClause);
      Assert.AreSame (queryBody, model.QueryBody);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialization_ThrowsOnNullFromClause ()
    {
      new QueryExpression (null, CreateQueryBody ());
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialization_ThrowsOnNullQueryBody ()
    {
      new QueryExpression (CreateFromClause(), null);
    }

    [Test]
    public void QueryExpression_ImplementsIQueryElement()
    {
      QueryExpression instance = new QueryExpression (CreateFromClause (), CreateQueryBody ());
      Assert.IsInstanceOfType (typeof (IQueryElement), instance);
    }

    [Test]
    public void Accept()
    {
      QueryExpression instance = new QueryExpression (CreateFromClause(), CreateQueryBody());

      MockRepository repository = new MockRepository ();
      IQueryVisitor testVisitor = repository.CreateMock<IQueryVisitor> ();

      //// expectations
      testVisitor.VisitQueryExpression (instance);

      repository.ReplayAll ();

      instance.Accept (testVisitor);

      repository.VerifyAll ();
    }

    private FromClause CreateFromClause ()
    {
      ParameterExpression id = Expression.Parameter (typeof (int), "i");
      Expression expression = Expression.NewArrayInit (typeof (int));
      return new FromClause (id, expression);
    }

    private QueryBody CreateQueryBody ()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      ISelectGroupClause iSelectGroupClause = new SelectClause (expression);
      return new QueryBody (iSelectGroupClause);
    }
  }
}
