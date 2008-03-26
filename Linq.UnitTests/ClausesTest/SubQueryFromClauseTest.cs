using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class SubQueryFromClauseTest
  {
    private IClause _previousClause;
    private ParameterExpression _identifier;
    private QueryExpression _subQuery;
    private SubQueryFromClause _subQueryFromClause;

    [SetUp]
    public void SetUp ()
    {
      _previousClause = ExpressionHelper.CreateMainFromClause ();
      _identifier = ExpressionHelper.CreateParameterExpression ();
      _subQuery = ExpressionHelper.CreateQueryExpression ();

      _subQueryFromClause = new SubQueryFromClause (_previousClause, _identifier, _subQuery);
    }

    [Test]
    public void Initialize()
    {

      Assert.AreSame (_previousClause, _subQueryFromClause.PreviousClause);
      Assert.AreSame (_identifier, _subQueryFromClause.Identifier);
      Assert.AreSame (_subQuery, _subQueryFromClause.SubQuery);
    }

    [Test]
    public void Accept ()
    {
      MockRepository mockRepository = new MockRepository();
      IQueryVisitor visitorMock = mockRepository.CreateMock<IQueryVisitor>();

      // expectation
      visitorMock.VisitSubQueryFromClause (_subQueryFromClause);

      mockRepository.ReplayAll ();
      _subQueryFromClause.Accept (visitorMock);
      mockRepository.VerifyAll ();
    }

    [Test]
    public void GetQueriedEntityType ()
    {
      Assert.AreEqual (null, _subQueryFromClause.GetQueriedEntityType ());
    }
  }
}