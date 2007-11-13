using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.Visitor;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class QueryExpressionTest
  {
    [Test]
    public void Initialize_WithFromClauseAndBody ()
    {
      FromClause fromClause = ExpressionHelper.CreateFromClause();
      QueryBody queryBody = ExpressionHelper.CreateQueryBody();
      QueryExpression model = new QueryExpression (fromClause, queryBody);
      Assert.AreSame (fromClause, model.FromClause);
      Assert.AreSame (queryBody, model.QueryBody);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialization_ThrowsOnNullFromClause ()
    {
      new QueryExpression (null, ExpressionHelper.CreateQueryBody ());
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialization_ThrowsOnNullQueryBody ()
    {
      new QueryExpression (ExpressionHelper.CreateFromClause(), null);
    }

    [Test]
    public void QueryExpression_ImplementsIQueryElement()
    {
      QueryExpression instance = new QueryExpression (ExpressionHelper.CreateFromClause (), ExpressionHelper.CreateQueryBody ());
      Assert.IsInstanceOfType (typeof (IQueryElement), instance);
    }

    [Test]
    public void Accept()
    {
      QueryExpression instance = new QueryExpression (ExpressionHelper.CreateFromClause(), ExpressionHelper.CreateQueryBody());

      MockRepository repository = new MockRepository ();
      IQueryVisitor testVisitor = repository.CreateMock<IQueryVisitor> ();

      //// expectations
      testVisitor.VisitQueryExpression (instance);

      repository.ReplayAll ();

      instance.Accept (testVisitor);

      repository.VerifyAll ();
    }

    [Test]
    public void Override_ToString()
    {
      QueryExpression queryExpression = new QueryExpression (ExpressionHelper.CreateFromClause (), ExpressionHelper.CreateQueryBody ());

      StringVisitor sv = new StringVisitor();

      sv.VisitQueryExpression (queryExpression);

      Assert.AreEqual (sv.ToString (), queryExpression.ToString ());

    }
    
  }
}
