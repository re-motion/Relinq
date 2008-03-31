using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using OrderDirection=Rubicon.Data.Linq.Clauses.OrderDirection;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class QueryExpression_BodyTest
  {
    private ISelectGroupClause _selectOrGroupClause;
    private QueryModel _model;

    [SetUp]
    public void SetUp()
    {
      _selectOrGroupClause = ExpressionHelper.CreateSelectClause ();
      _model = new QueryModel (typeof (IQueryable<int>), ExpressionHelper.CreateMainFromClause(), _selectOrGroupClause);
    }

    [Test]
    public void InitializeWithISelectOrGroupClauseAndOrderByClause()
    {
      LambdaExpression orderingExpression = ExpressionHelper.CreateLambdaExpression ();
      OrderingClause ordering = new OrderingClause (ExpressionHelper.CreateClause(),orderingExpression, OrderDirection.Asc);

      OrderByClause orderByClause = new OrderByClause (ordering);
      _model.AddBodyClause (orderByClause);

      Assert.AreSame (_selectOrGroupClause, _model.SelectOrGroupClause);
      Assert.AreEqual (1, _model.BodyClauses.Count);
      Assert.That (_model.BodyClauses, List.Contains (orderByClause));
    }

    [Test]
    public void AddSeveralOrderByClauses()
    {
 
      IBodyClause orderByClause1 = ExpressionHelper.CreateOrderByClause();
      IBodyClause orderByClause2 = ExpressionHelper.CreateOrderByClause ();

      _model.AddBodyClause (orderByClause1);
      _model.AddBodyClause (orderByClause2);

      Assert.AreEqual (2, _model.BodyClauses.Count);
      Assert.That (_model.BodyClauses, Is.EqualTo (new object[] { orderByClause1, orderByClause2 }));
    }

    
    [Test]
    public void AddBodyClause()
    {
      IBodyClause clause = ExpressionHelper.CreateWhereClause();
      _model.AddBodyClause (clause);

      Assert.AreEqual (1, _model.BodyClauses.Count);
      Assert.That (_model.BodyClauses, List.Contains (clause));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Multiple from clauses with the same name ('s') are not supported.")]
    public void AddFromClausesWithSameIdentifiers ()
    {
      IClause previousClause = ExpressionHelper.CreateClause();
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression ();

      _model.AddBodyClause (new AdditionalFromClause (previousClause, identifier, fromExpression, projectionExpression));
      _model.AddBodyClause (new AdditionalFromClause (previousClause, identifier, fromExpression, projectionExpression));
    }
    
    [Test]
    public void GetAdditionalFromClause()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression identifier3 = Expression.Parameter (typeof (Student), "s3");

      AdditionalFromClause clause1 = new AdditionalFromClause (ExpressionHelper.CreateMainFromClause (), identifier1, fromExpression, projExpression);
      AdditionalFromClause clause2 = new AdditionalFromClause (clause1, identifier2, fromExpression, projExpression);
      AdditionalFromClause clause3 = new AdditionalFromClause (clause2, identifier3, fromExpression, projExpression);

      _model.AddBodyClause (clause1);
      _model.AddBodyClause (clause2);
      _model.AddBodyClause (clause3);

      Assert.AreSame (clause1, _model.GetAdditionalFromClause ("s1", typeof (Student)));
      Assert.AreSame (clause2, _model.GetAdditionalFromClause ("s2", typeof (Student)));
      Assert.AreSame (clause3, _model.GetAdditionalFromClause ("s3", typeof (Student)));
    }

    [Test]
    public void GetAdditionalFromClause_InvalidName ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      AdditionalFromClause clause1 = new AdditionalFromClause (ExpressionHelper.CreateMainFromClause (), identifier1, fromExpression, projExpression);

      _model.AddBodyClause (clause1);

      Assert.IsNull (_model.GetAdditionalFromClause ("fzlbf", typeof (Student)));
    }

    [Test]
    [ExpectedException (typeof (ClauseLookupException), ExpectedMessage = "The from clause with identifier 's1' has type "
        + "'Rubicon.Data.Linq.UnitTests.Student', but 'System.String' was requested.")]
    public void GetAdditionalFromClause_InvalidType ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      AdditionalFromClause clause1 = new AdditionalFromClause (ExpressionHelper.CreateMainFromClause (), identifier1, fromExpression, projExpression);

      _model.AddBodyClause (clause1);
      _model.GetAdditionalFromClause ("s1", typeof (string));
    }
  }
}