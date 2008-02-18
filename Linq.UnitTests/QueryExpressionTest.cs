using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
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
      Assert.AreSame (fromClause, model.MainFromClause);
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
      Assert.AreSame (fromClause, model.MainFromClause);
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

    [Test]
    public void GetFromClause ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier0 = Expression.Parameter (typeof (Student), "s0");
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression identifier3 = Expression.Parameter (typeof (Student), "s3");

      MainFromClause mainFromClause = new MainFromClause (identifier0, ExpressionHelper.CreateQuerySource());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);
      AdditionalFromClause clause2 = new AdditionalFromClause (clause1, identifier2, fromExpression, projExpression);
      AdditionalFromClause clause3 = new AdditionalFromClause (clause2, identifier3, fromExpression, projExpression);

      QueryBody body = new QueryBody (ExpressionHelper.CreateSelectClause ());
      body.Add (clause1);
      body.Add (clause2);
      body.Add (clause3);

      QueryExpression expression = new QueryExpression (mainFromClause, body);

      Assert.AreSame (mainFromClause, expression.GetFromClause ("s0", typeof (Student)));
      Assert.AreSame (clause1, expression.GetFromClause ("s1", typeof (Student)));
      Assert.AreSame (clause2, expression.GetFromClause ("s2", typeof (Student)));
      Assert.AreSame (clause3, expression.GetFromClause ("s3", typeof (Student)));
    }

    [Test]
    public void GetFromClause_InvalidIdentifierName ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier0 = Expression.Parameter (typeof (Student), "s0");
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");

      MainFromClause mainFromClause = new MainFromClause (identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryBody body = new QueryBody (ExpressionHelper.CreateSelectClause ());
      body.Add (clause1);

      QueryExpression expression = new QueryExpression (mainFromClause, body);

      Assert.IsNull (expression.GetFromClause ("s273627", typeof (Student)));
    }

    [Test]
    [ExpectedException (typeof (ClauseLookupException), ExpectedMessage = "The from clause with identifier 's0' has type "
        + "'Rubicon.Data.Linq.UnitTests.Student', but 'System.String' was requested.")]
    public void GetFromClause_InvalidIdentifierType_MainFromClause ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier0 = Expression.Parameter (typeof (Student), "s0");
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");

      MainFromClause mainFromClause = new MainFromClause (identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryBody body = new QueryBody (ExpressionHelper.CreateSelectClause ());
      body.Add (clause1);

      QueryExpression expression = new QueryExpression (mainFromClause, body);

      expression.GetFromClause ("s0", typeof (string));
      Assert.Fail ("Expected exception");
    }

    [Test]
    [ExpectedException (typeof (ClauseLookupException), ExpectedMessage = "The from clause with identifier 's1' has type "
        + "'Rubicon.Data.Linq.UnitTests.Student', but 'System.String' was requested.")]
    public void GetFromClause_InvalidIdentifierType_AdditionalFromClause ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier0 = Expression.Parameter (typeof (Student), "s0");
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");

      MainFromClause mainFromClause = new MainFromClause (identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryBody body = new QueryBody (ExpressionHelper.CreateSelectClause ());
      body.Add (clause1);

      QueryExpression expression = new QueryExpression (mainFromClause, body);

      expression.GetFromClause ("s1", typeof (string));
      Assert.Fail ("Expected exception");
    }

    [Test]
    public void ResolveField()
    {
      QueryExpression queryExpression = CreateQueryExpressionForResolve();

      Expression fieldAccessExpression = Expression.Parameter (typeof (String), "s1");
      JoinedTableContext context = new JoinedTableContext ();     
      FieldDescriptor descriptor = queryExpression.ResolveField (StubDatabaseInfo.Instance, context, fieldAccessExpression);

      Table expectedTable = new Table ("studentTable", "s1");
      Assert.AreSame (queryExpression.MainFromClause, descriptor.FromClause);
      Assert.AreEqual (new Column (expectedTable, "*"), descriptor.Column);
      Assert.IsNull (descriptor.Member);
      Assert.AreEqual (expectedTable, descriptor.SourcePath);
    }

    private QueryExpression CreateQueryExpressionForResolve ()
    {
      ParameterExpression s1 = Expression.Parameter (typeof (String), "s1");
      ParameterExpression s2 = Expression.Parameter (typeof (String), "s2");
      MainFromClause mainFromClause = new MainFromClause (s1, ExpressionHelper.CreateQuerySource());
      AdditionalFromClause additionalFromClause =
          new AdditionalFromClause (mainFromClause, s2, ExpressionHelper.CreateLambdaExpression(), ExpressionHelper.CreateLambdaExpression());
      
      QueryBody queryBody = new QueryBody (ExpressionHelper.CreateSelectClause());
      queryBody.Add (additionalFromClause);

      return new QueryExpression (mainFromClause, queryBody);
    }
  }
}
