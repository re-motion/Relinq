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
      Expression expression = ExpressionHelper.CreateExpression();

      AdditionalFromClause fromClause = new AdditionalFromClause (id, expression);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (expression, fromClause.Expression);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauseCount);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialize_ThrowsOnNullID ()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      new AdditionalFromClause (null, expression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialize_ThrowsOnNullExpressions ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      new AdditionalFromClause (id, null);
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