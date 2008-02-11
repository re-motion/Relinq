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
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Multiple from clauses with the same name ('s') are not supported.")]
    public void AddFromClausesWithSameIdentifiers ()
    {
      ISelectGroupClause iSelectOrGroupClause = ExpressionHelper.CreateSelectClause ();
      QueryBody queryBody = new QueryBody (iSelectOrGroupClause);

      IClause previousClause = ExpressionHelper.CreateClause();
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression ();

      queryBody.Add (new AdditionalFromClause (previousClause, identifier, fromExpression, projectionExpression));
      queryBody.Add (new AdditionalFromClause (previousClause, identifier, fromExpression, projectionExpression));
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

    [Test]
    public void GetFromClause()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression identifier3 = Expression.Parameter (typeof (Student), "s3");

      AdditionalFromClause clause1 = new AdditionalFromClause (ExpressionHelper.CreateMainFromClause (), identifier1, fromExpression, projExpression);
      AdditionalFromClause clause2 = new AdditionalFromClause (clause1, identifier2, fromExpression, projExpression);
      AdditionalFromClause clause3 = new AdditionalFromClause (clause2, identifier3, fromExpression, projExpression);

      QueryBody body = new QueryBody (ExpressionHelper.CreateSelectClause());
      body.Add (clause1);
      body.Add (clause2);
      body.Add (clause3);

      Assert.AreSame (clause1, body.GetFromClause ("s1", typeof (Student)));
      Assert.AreSame (clause2, body.GetFromClause ("s2", typeof (Student)));
      Assert.AreSame (clause3, body.GetFromClause ("s3", typeof (Student)));
    }

    [Test]
    public void GetFromClause_InvalidName ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");

      AdditionalFromClause clause1 = new AdditionalFromClause (ExpressionHelper.CreateMainFromClause (), identifier1, fromExpression, projExpression);

      QueryBody body = new QueryBody (ExpressionHelper.CreateSelectClause ());
      body.Add (clause1);

      Assert.IsNull (body.GetFromClause ("fzlbf", typeof (Student)));
    }

    [Test]
    [ExpectedException (typeof (ClauseLookupException), ExpectedMessage = "The from clause with identifier 's1' has type "
        + "'Rubicon.Data.Linq.UnitTests.Student', but 'System.String' was requested.")]
    public void GetFromClause_InvalidType ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");

      AdditionalFromClause clause1 = new AdditionalFromClause (ExpressionHelper.CreateMainFromClause (), identifier1, fromExpression, projExpression);

      QueryBody body = new QueryBody (ExpressionHelper.CreateSelectClause ());
      body.Add (clause1);

      body.GetFromClause ("s1", typeof (string));
    }
  }
}