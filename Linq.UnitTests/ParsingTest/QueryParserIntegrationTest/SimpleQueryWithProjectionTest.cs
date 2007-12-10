using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class SimpleQueryWithProjectionTest: QueryTestBase<string>
  {
    protected override IQueryable<string> CreateQuery ()
    {
      return TestQueryGenerator.CreateSimpleQueryWithProjection (QuerySource);
    }

    [Test]
    public override void CheckFromLetWhereClauses ()
    {
      Assert.AreEqual (0, ParsedQuery.QueryBody.FromLetWhereClauseCount);
    }

    [Test]
    public override void CheckOrderByClause ()
    {
      Assert.IsNull (ParsedQuery.QueryBody.OrderByClause);
    }

    [Test]
    public override void CheckSelectOrGroupClause ()
    {
      Assert.IsNotNull (ParsedQuery.QueryBody.SelectOrGroupClause);
      SelectClause clause = ParsedQuery.QueryBody.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNotNull (clause.ProjectionExpression);
      Assert.IsInstanceOfType (typeof(MemberExpression), clause.ProjectionExpression.Body,
          "from s in ... select s.First => select expression must be MemberAccess");
    }
  }
}