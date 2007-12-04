using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class MainFromClauseTest
  {
    [Test]
    public void Initialize_WithIDAndExpression ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();

      MainFromClause fromClause = new MainFromClause (id, querySource);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (querySource, fromClause.QuerySource);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauseCount);

      Assert.IsNull (fromClause.PreviousClause);
    }

    [Test]
    public void Accept ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();

      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor> ();

      visitorMock.VisitMainFromClause (fromClause);

      repository.ReplayAll ();

      fromClause.Accept (visitorMock);

      repository.VerifyAll ();

    }
  }
}