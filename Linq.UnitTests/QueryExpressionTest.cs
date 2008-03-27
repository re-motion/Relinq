using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Data.Linq.Visitor;
using Rubicon.Data.Linq.UnitTests.TestQueryGenerators;

namespace Rubicon.Data.Linq.UnitTests
{
  [TestFixture]
  public class QueryExpressionTest
  {
    [Test]
    public void Initialize_WithFromClauseAndBody ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      SelectClause selectClause = ExpressionHelper.CreateSelectClause ();
      QueryExpression model = new QueryExpression (typeof (IQueryable<string>), fromClause, selectClause);
      Assert.AreSame (fromClause, model.MainFromClause);
      Assert.AreSame (selectClause, model.SelectOrGroupClause);
      Assert.IsNotNull (model.GetExpressionTree ());
      Assert.AreEqual (typeof (IQueryable<string>), model.ResultType);
    }

    [Test]
    public void Initialize_WithExpressionTree ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      Expression expressionTree = ExpressionHelper.CreateExpression();
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();
      QueryExpression model = new QueryExpression (typeof (IQueryable<string>), fromClause, selectClause, expressionTree);
      Assert.AreSame (fromClause, model.MainFromClause);
      Assert.AreSame (selectClause, model.SelectOrGroupClause);
      Assert.AreSame (expressionTree, model.GetExpressionTree());
    }

    [Test]
    public void QueryExpression_ImplementsIQueryElement()
    {
      QueryExpression instance = ExpressionHelper.CreateQueryExpression();
      Assert.IsInstanceOfType (typeof (IQueryElement), instance);
    }

    [Test]
    public void Accept()
    {
      QueryExpression instance = ExpressionHelper.CreateQueryExpression();

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
      QueryExpression queryExpression = ExpressionHelper.CreateQueryExpression();

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
      SelectClause selectClause = new SelectClause (fromClause, Expression.Lambda (Expression.Constant (0)),false);

      QueryExpression queryExpression = new QueryExpression (typeof (IQueryable<int>), fromClause, selectClause);
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

      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(identifier0, ExpressionHelper.CreateQuerySource());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);
      AdditionalFromClause clause2 = new AdditionalFromClause (clause1, identifier2, fromExpression, projExpression);
      AdditionalFromClause clause3 = new AdditionalFromClause (clause2, identifier3, fromExpression, projExpression);


      QueryExpression expression = ExpressionHelper.CreateQueryExpression (mainFromClause);
      expression.AddBodyClause (clause1);
      expression.AddBodyClause (clause2);
      expression.AddBodyClause (clause3);

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

      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryExpression expression = ExpressionHelper.CreateQueryExpression (mainFromClause);
      expression.AddBodyClause (clause1);

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

      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryExpression expression = ExpressionHelper.CreateQueryExpression (mainFromClause);
      expression.AddBodyClause (clause1);

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

      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryExpression expression = ExpressionHelper.CreateQueryExpression (mainFromClause);
      expression.AddBodyClause (clause1);

      expression.GetFromClause ("s1", typeof (string));
      Assert.Fail ("Expected exception");
    }

    [Test]
    public void ResolveField ()
    {
      QueryExpression queryExpression = CreateQueryExpressionForResolve ();

      Expression fieldAccessExpression = Expression.Parameter (typeof (String), "s1");
      JoinedTableContext context = new JoinedTableContext ();
      WhereFieldAccessPolicy policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      FromClauseFieldResolver resolver = new FromClauseFieldResolver (StubDatabaseInfo.Instance, context, policy);
      FieldDescriptor descriptor = queryExpression.ResolveField (resolver, fieldAccessExpression);

      IFromSource expectedTable = queryExpression.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      FieldSourcePath expectedPath = new FieldSourcePath (expectedTable, new SingleJoin[0]);

      Assert.AreSame (queryExpression.MainFromClause, descriptor.FromClause);
      Assert.AreEqual (new Column (expectedTable, "*"), descriptor.Column);
      Assert.IsNull (descriptor.Member);
      Assert.AreEqual (expectedPath, descriptor.SourcePath);
    }

    [Test]
    public void ParentQuery_Null ()
    {
      QueryExpression queryExpression = ExpressionHelper.CreateQueryExpression();
      Assert.IsNull (queryExpression.ParentQuery);
    }

    [Test]
    public void SetParentQuery ()
    {
      QueryExpression queryExpression = ExpressionHelper.CreateQueryExpression ();
      QueryExpression parentQueryExpression = ExpressionHelper.CreateQueryExpression ();
      queryExpression.SetParentQuery (parentQueryExpression);

      Assert.AreSame (parentQueryExpression, queryExpression.ParentQuery);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The query already has a parent query.")]
    public void SetParentQuery_ThrowsOnSecondParent ()
    {
      QueryExpression queryExpression = ExpressionHelper.CreateQueryExpression ();
      QueryExpression parentQueryExpression = ExpressionHelper.CreateQueryExpression ();
      queryExpression.SetParentQuery (parentQueryExpression);
      queryExpression.SetParentQuery (parentQueryExpression);
    }

    private QueryExpression CreateQueryExpressionForResolve ()
    {
      ParameterExpression s1 = Expression.Parameter (typeof (String), "s1");
      ParameterExpression s2 = Expression.Parameter (typeof (String), "s2");
      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(s1, ExpressionHelper.CreateQuerySource());
      AdditionalFromClause additionalFromClause =
          new AdditionalFromClause (mainFromClause, s2, ExpressionHelper.CreateLambdaExpression(), ExpressionHelper.CreateLambdaExpression());

      QueryExpression queryExpression = ExpressionHelper.CreateQueryExpression (mainFromClause);
      queryExpression.AddBodyClause (additionalFromClause);
      return queryExpression;
    }
  }
}
