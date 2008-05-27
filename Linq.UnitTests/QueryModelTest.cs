using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.Visitor;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests
{
  [TestFixture]
  public class QueryModelTest
  {
    [Test]
    public void Initialize_WithFromClauseAndBody ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      SelectClause selectClause = ExpressionHelper.CreateSelectClause ();
      QueryModel model = new QueryModel (typeof (IQueryable<string>), fromClause, selectClause);
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
      QueryModel model = new QueryModel (typeof (IQueryable<string>), fromClause, selectClause, expressionTree);
      Assert.AreSame (fromClause, model.MainFromClause);
      Assert.AreSame (selectClause, model.SelectOrGroupClause);
      Assert.AreSame (expressionTree, model.GetExpressionTree());
    }

    [Test]
    public void QueryExpression_ImplementsIQueryElement()
    {
      QueryModel instance = ExpressionHelper.CreateQueryModel();
      Assert.IsInstanceOfType (typeof (IQueryElement), instance);
    }

    [Test]
    public void Accept()
    {
      QueryModel instance = ExpressionHelper.CreateQueryModel();

      MockRepository repository = new MockRepository ();
      IQueryVisitor testVisitor = repository.CreateMock<IQueryVisitor> ();

      //// expectations
      testVisitor.VisitQueryModel (instance);

      repository.ReplayAll ();

      instance.Accept (testVisitor);

      repository.VerifyAll ();
    }

    [Test]
    public void Override_ToString()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel();

      StringVisitor sv = new StringVisitor();

      sv.VisitQueryModel (queryModel);

      Assert.AreEqual (sv.ToString (), queryModel.ToString ());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The query contains an invalid query method",
        MatchType = MessageMatch.Contains)]
    public void GetExpressionTree_ThrowsOnInvalidSelectCall()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      SelectClause selectClause = new SelectClause (fromClause, Expression.Lambda (Expression.Constant (0)),false);

      QueryModel queryModel = new QueryModel (typeof (IQueryable<int>), fromClause, selectClause);
      queryModel.GetExpressionTree();
    }

    [Test]
    public void GetResolveableClause_FromClauseBase ()
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


      QueryModel model = ExpressionHelper.CreateQueryModel (mainFromClause);
      model.AddBodyClause (clause1);
      model.AddBodyClause (clause2);
      model.AddBodyClause (clause3);

      Assert.AreSame (mainFromClause, model.GetResolveableClause ("s0", typeof (Student)));
      Assert.AreSame (clause1, model.GetResolveableClause ("s1", typeof (Student)));
      Assert.AreSame (clause2, model.GetResolveableClause ("s2", typeof (Student)));
      Assert.AreSame (clause3, model.GetResolveableClause ("s3", typeof (Student)));
    }

    [Test]
    public void GetResolveableClause_InvalidIdentifierName ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier0 = Expression.Parameter (typeof (Student), "s0");
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");

      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryModel model = ExpressionHelper.CreateQueryModel (mainFromClause);
      model.AddBodyClause (clause1);

      Assert.IsNull (model.GetResolveableClause ("s273627", typeof (Student)));
    }
   
    [Test]
    [ExpectedException (typeof (ClauseLookupException), ExpectedMessage = "The from clause with identifier 's0' has type "
        + "'Remotion.Data.Linq.UnitTests.Student', but 'System.String' was requested.")]
    public void GetResolveableClause_InvalidIdentifierType_MainFromClause ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier0 = Expression.Parameter (typeof (Student), "s0");
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");

      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryModel model = ExpressionHelper.CreateQueryModel (mainFromClause);
      model.AddBodyClause (clause1);

      model.GetResolveableClause ("s0", typeof (string));
      Assert.Fail ("Expected exception");
    }

    [Test]
    [ExpectedException (typeof (ClauseLookupException), ExpectedMessage = "The from clause with identifier 's1' has type "
        + "'Remotion.Data.Linq.UnitTests.Student', but 'System.String' was requested.")]
    public void GetResolveableClause_InvalidIdentifierType_AdditionalFromClause ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier0 = Expression.Parameter (typeof (Student), "s0");
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");

      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryModel model = ExpressionHelper.CreateQueryModel (mainFromClause);
      model.AddBodyClause (clause1);

      model.GetResolveableClause ("s1", typeof (string));
      Assert.Fail ("Expected exception");
    }

    [Test]
    public void GetResolveableClause_LetClause ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s0");
      
      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      LetClause letClause = ExpressionHelper.CreateLetClause ();

      QueryModel model = ExpressionHelper.CreateQueryModel (mainFromClause);
      model.AddBodyClause (letClause);

      Assert.AreSame (letClause, model.GetResolveableClause ("i", typeof (int)));
    }
    
    [Test]
    public void ResolveField ()
    {
      QueryModel queryModel = CreateQueryExpressionForResolve ();

      Expression fieldAccessExpression = Expression.Parameter (typeof (String), "s1");
      JoinedTableContext context = new JoinedTableContext ();
      WhereFieldAccessPolicy policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      ClauseFieldResolver resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, context, policy);
      FieldDescriptor descriptor = queryModel.ResolveField (resolver, fieldAccessExpression);

      IColumnSource expectedTable = queryModel.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      FieldSourcePath expectedPath = new FieldSourcePath (expectedTable, new SingleJoin[0]);

      //Assert.AreSame (queryModel.MainFromClause, descriptor.FromClause);
      Assert.AreEqual (new Column (expectedTable, "*"), descriptor.Column);
      Assert.IsNull (descriptor.Member);
      Assert.AreEqual (expectedPath, descriptor.SourcePath);
    }

    [Test]
    public void ParentQuery_Null ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel();
      Assert.IsNull (queryModel.ParentQuery);
    }

    [Test]
    public void SetParentQuery ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      QueryModel parentQueryModel = ExpressionHelper.CreateQueryModel ();
      queryModel.SetParentQuery (parentQueryModel);

      Assert.AreSame (parentQueryModel, queryModel.ParentQuery);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The query already has a parent query.")]
    public void SetParentQuery_ThrowsOnSecondParent ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      QueryModel parentQueryModel = ExpressionHelper.CreateQueryModel ();
      queryModel.SetParentQuery (parentQueryModel);
      queryModel.SetParentQuery (parentQueryModel);
    }

    
    private QueryModel CreateQueryExpressionForResolve ()
    {
      ParameterExpression s1 = Expression.Parameter (typeof (String), "s1");
      ParameterExpression s2 = Expression.Parameter (typeof (String), "s2");
      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(s1, ExpressionHelper.CreateQuerySource());
      AdditionalFromClause additionalFromClause =
          new AdditionalFromClause (mainFromClause, s2, ExpressionHelper.CreateLambdaExpression(), ExpressionHelper.CreateLambdaExpression());

      QueryModel queryModel = ExpressionHelper.CreateQueryModel (mainFromClause);
      queryModel.AddBodyClause (additionalFromClause);
      return queryModel;
    }
  }
}
