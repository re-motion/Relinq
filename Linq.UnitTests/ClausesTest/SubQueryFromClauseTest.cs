using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class SubQueryFromClauseTest
  {
    private IClause _previousClause;
    private ParameterExpression _identifier;
    private QueryExpression _subQueryExpression;
    private SubQueryFromClause _subQueryFromClause;
    private LambdaExpression _projectionExpression;

    [SetUp]
    public void SetUp ()
    {
      _previousClause = ExpressionHelper.CreateMainFromClause ();
      _identifier = ExpressionHelper.CreateParameterExpression ();
      _subQueryExpression = ExpressionHelper.CreateQueryExpression ();
      _projectionExpression = ExpressionHelper.CreateLambdaExpression();

      _subQueryFromClause = new SubQueryFromClause (_previousClause, _identifier, _subQueryExpression, _projectionExpression);
    }

    [Test]
    public void Initialize()
    {
      Assert.AreSame (_previousClause, _subQueryFromClause.PreviousClause);
      Assert.AreSame (_identifier, _subQueryFromClause.Identifier);
      Assert.AreSame (_subQueryExpression, _subQueryFromClause.SubQueryExpression);
      Assert.AreSame (_projectionExpression, _subQueryFromClause.ProjectionExpression);
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

    [Test]
    public void GetFromSource ()
    {
      IFromSource fromSource = _subQueryFromClause.GetFromSource (StubDatabaseInfo.Instance);
      SubQuery subQuery = (SubQuery) fromSource;
      Assert.AreEqual (_identifier.Name, subQuery.Alias);
      Assert.AreSame (_subQueryExpression, subQuery.QueryExpression);
    }

    [Test]
    public void GetFromSource_FromSourceIsCached ()
    {
      SubQuery subQuery1 = (SubQuery) _subQueryFromClause.GetFromSource (StubDatabaseInfo.Instance);
      SubQuery subQuery2 = (SubQuery) _subQueryFromClause.GetFromSource (StubDatabaseInfo.Instance);
      Assert.AreSame (subQuery1, subQuery2);
    }
  }
}