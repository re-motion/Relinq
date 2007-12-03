using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;

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

      AdditionalFromClause fromClause = new AdditionalFromClause (id, fromExpression, projectionExpression);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (fromExpression, fromClause.FromExpression);
      Assert.AreSame (projectionExpression, fromClause.ProjectionExpression);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauseCount);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialize_ThrowsOnNullID ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression ();
      new AdditionalFromClause (null, expression, projectionExpression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialize_ThrowsOnNullFromExpressions ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression ();
      new AdditionalFromClause (id, null, projectionExpression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialize_ThrowsOnNullProjectionExpressions ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      new AdditionalFromClause (id, expression, null);
    }

    [Test]
    public void ImplementInterface_IFromLetWhereClause ()
    {
      AdditionalFromClause fromClause = ExpressionHelper.CreateAdditionalFromClause ();
      Assert.IsInstanceOfType (typeof (IFromLetWhereClause), fromClause);
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