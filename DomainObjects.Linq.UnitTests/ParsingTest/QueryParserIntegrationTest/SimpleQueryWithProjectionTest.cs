using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest
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
      Assert.IsNotNull (clause.ProjectionExpressions);
      Assert.IsInstanceOfType (typeof(MemberExpression) ,clause.ProjectionExpressions[0].Body,
          "from s in ... select s.First => select expression must be MemberAccess");
    }
  }
}