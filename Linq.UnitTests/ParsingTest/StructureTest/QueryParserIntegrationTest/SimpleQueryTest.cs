using System;
using System.Linq;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class SimpleQueryTest : QueryTestBase<Student>
  {
    protected override IQueryable<Student> CreateQuery ()
    {
      return TestQueryGenerator.CreateSimpleQuery (QuerySource);
    }

    [Test]
    public override void CheckBodyClause ()
    {
      Assert.AreEqual (0, ParsedQuery.QueryBody.BodyClauses.Count);
    }

    [Test]
    public override void CheckSelectOrGroupClause ()
    {
      Assert.IsNotNull (ParsedQuery.QueryBody.SelectOrGroupClause);
      SelectClause clause = ParsedQuery.QueryBody.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNotNull (clause.ProjectionExpression);
      Assert.AreSame (ParsedQuery.MainFromClause.Identifier, clause.ProjectionExpression.Body,
          "from s in ... select s => select expression must be same as from-identifier");
    }
  }
}