using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Visitor;

namespace Rubicon.Data.Linq.UnitTests
{
  [TestFixture]
  public class QueryExpressionTest
  {
    [Test]
    public void Initialize_WithFromClauseAndBody ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      QueryBody queryBody = ExpressionHelper.CreateQueryBody();
      QueryExpression model = new QueryExpression (fromClause, queryBody);
      Assert.AreSame (fromClause, model.FromClause);
      Assert.AreSame (queryBody, model.QueryBody);
      Assert.IsNotNull (model.GetExpressionTree());
    }

    [Test]
    public void Initialize_WithExpressionTree ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      QueryBody queryBody = ExpressionHelper.CreateQueryBody ();
      Expression expressionTree = ExpressionHelper.CreateExpression();
      QueryExpression model = new QueryExpression (fromClause, queryBody,expressionTree);
      Assert.AreSame (fromClause, model.FromClause);
      Assert.AreSame (queryBody, model.QueryBody);
      Assert.AreSame (expressionTree, model.GetExpressionTree());
    }

    [Test]
    public void QueryExpression_ImplementsIQueryElement()
    {
      QueryExpression instance = new QueryExpression (ExpressionHelper.CreateMainFromClause (), ExpressionHelper.CreateQueryBody ());
      Assert.IsInstanceOfType (typeof (IQueryElement), instance);
    }

    [Test]
    public void Accept()
    {
      QueryExpression instance = new QueryExpression (ExpressionHelper.CreateMainFromClause(), ExpressionHelper.CreateQueryBody());

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
      QueryExpression queryExpression = new QueryExpression (ExpressionHelper.CreateMainFromClause (), ExpressionHelper.CreateQueryBody ());

      StringVisitor sv = new StringVisitor();

      sv.VisitQueryExpression (queryExpression);

      Assert.AreEqual (sv.ToString (), queryExpression.ToString ());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The query contains an invalid query method",
        MatchType = MessageMatch.Contains)]
    public void GetExpressionTree_ThrowsOnInvalidSelectCall()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      SelectClause selectClause = new SelectClause (fromClause, Expression.Lambda (Expression.Constant (0)));
      QueryBody queryBody = new QueryBody (selectClause);

      QueryExpression queryExpression = new QueryExpression (fromClause, queryBody);
      queryExpression.GetExpressionTree();
    }
  }
}
