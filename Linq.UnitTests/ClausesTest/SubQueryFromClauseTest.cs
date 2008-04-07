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
    private QueryModel _subQueryModel;
    private SubQueryFromClause _subQueryFromClause;
    private LambdaExpression _projectionExpression;

    [SetUp]
    public void SetUp ()
    {
      _previousClause = ExpressionHelper.CreateMainFromClause ();
      _identifier = ExpressionHelper.CreateParameterExpression ();
      _subQueryModel = ExpressionHelper.CreateQueryModel ();
      _projectionExpression = ExpressionHelper.CreateLambdaExpression();

      _subQueryFromClause = new SubQueryFromClause (_previousClause, _identifier, _subQueryModel, _projectionExpression);
    }

    [Test]
    public void Initialize()
    {
      Assert.AreSame (_previousClause, _subQueryFromClause.PreviousClause);
      Assert.AreSame (_identifier, _subQueryFromClause.Identifier);
      Assert.AreSame (_subQueryModel, _subQueryFromClause.SubQueryModel);
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
      Assert.AreEqual (null, _subQueryFromClause.GetQuerySourceType ());
    }

    [Test]
    public void GetFromSource ()
    {
      IColumnSource columnSource = _subQueryFromClause.GetFromSource (StubDatabaseInfo.Instance);
      SubQuery subQuery = (SubQuery) columnSource;
      Assert.AreEqual (_identifier.Name, subQuery.Alias);
      Assert.AreSame (_subQueryModel, subQuery.QueryModel);
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