using System.Linq.Expressions;
using NUnit.Framework;
using System.Linq;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.Parsing.Structure;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest
{
  [TestFixture]
  public class SourceExpressionParserTest
  {
    private SourceExpressionParser _topLevelParser;
    private IQueryable<Student> _source;

    [SetUp]
    public void SetUp()
    {
      _topLevelParser = new SourceExpressionParser (true);
      _source = ExpressionHelper.CreateQuerySource();
    }

    [Test]
    public void NonDistinct ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (_source);
      ParseResultCollector result = Parse (query);
      Assert.IsFalse (result.IsDistinct);
    }

    [Test]
    public void Distinct_TopLevelBeforeSelect()
    {
      IQueryable<string> query = TestQueryGenerator.CreateSimpleDisinctQuery (_source);
      ParseResultCollector result = Parse (query);
      Assert.IsTrue (result.IsDistinct);
    }

    [Test]
    public void Distinct_TopLevelBeforeWhere ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateDisinctWithWhereQueryWithoutProjection (_source);
      ParseResultCollector result = Parse(query);
      Assert.IsTrue (result.IsDistinct);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Distinct is only allowed at the top level of a query, not in the middle: "
        + "'value(Rubicon.Data.Linq.UnitTests.TestQueryable`1[Rubicon.Data.Linq.UnitTests.Student]).Select(s => s).Distinct().Where(s => s.IsOld)'.")]
    public void Distinct_NonTopLevel ()
    {
      IQueryable<Student> query = _source.Select (s => s).Distinct().Where (s => s.IsOld);
      Parse (query);
    }

    private ParseResultCollector Parse (IQueryable query)
    {
      ParseResultCollector result = new ParseResultCollector (query.Expression);
      _topLevelParser.Parse (result, query.Expression, null, "bla");
      return result;
    }

  }
}