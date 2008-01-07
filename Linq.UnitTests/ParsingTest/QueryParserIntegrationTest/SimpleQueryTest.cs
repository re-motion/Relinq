using System;
using System.Linq;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest
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
      Assert.AreEqual (0, ParsedQuery.QueryBody.BodyClauseCount);
    }

   [Test]
    public override void CheckSelectOrGroupClause ()
    {
      Assert.IsNotNull (ParsedQuery.QueryBody.SelectOrGroupClause);
      SelectClause clause = ParsedQuery.QueryBody.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNotNull (clause.ProjectionExpression);
      Assert.AreSame (ParsedQuery.FromClause.Identifier, clause.ProjectionExpression.Body,
                      "from s in ... select s => select expression must be same as from-identifier");
    }
  }
}