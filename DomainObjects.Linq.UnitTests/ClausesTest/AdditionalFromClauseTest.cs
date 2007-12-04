using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.Parsing;
using Rubicon.Data.DomainObjects.Linq.QueryProviderImplementation;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class AdditionalFromClauseTest
  {
    [Test]
    public void Initialize_WithIDAndExpression ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression ();

      IClause clause = ExpressionHelper.CreateClause();
      
      AdditionalFromClause fromClause = new AdditionalFromClause (clause,id, fromExpression, projectionExpression);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (fromExpression, fromClause.FromExpression);
      Assert.AreSame (projectionExpression, fromClause.ProjectionExpression);
      Assert.AreSame (clause, fromClause.PreviousClause);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauseCount);
    }

    [Test]
    public void ImplementInterface_IFromLetWhereClause ()
    {
      AdditionalFromClause fromClause = ExpressionHelper.CreateAdditionalFromClause ();
      Assert.IsInstanceOfType (typeof (IFromLetWhereClause), fromClause);
    }

    [Test]
    public void GetQuerySource()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource();
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (querySource), Expression.Parameter (typeof (Student), "student"));
      AdditionalFromClause fromClause =
          new AdditionalFromClause (ExpressionHelper.CreateClause(), ExpressionHelper.CreateParameterExpression(), 
          fromExpression, ExpressionHelper.CreateLambdaExpression());
      Assert.AreSame (typeof (StandardQueryable<Student>), fromClause.GetQuerySourceType());
    }

    
    [Test]
    public void Accept ()
    {
      AdditionalFromClause fromClause = ExpressionHelper.CreateAdditionalFromClause ();

      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor> ();

      visitorMock.VisitAdditionalFromClause (fromClause);

      repository.ReplayAll ();

      fromClause.Accept (visitorMock);

      repository.VerifyAll ();

    }
  }
}