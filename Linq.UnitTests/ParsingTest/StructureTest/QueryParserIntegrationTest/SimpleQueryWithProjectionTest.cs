using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class SimpleQueryWithProjectionTest: QueryTestBase<string>
  {
    protected override IQueryable<string> CreateQuery ()
    {
      return TestQueryGenerator.CreateSimpleQueryWithProjection (QuerySource);
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
      Assert.IsInstanceOfType (typeof(MemberExpression), clause.ProjectionExpression.Body,
          "from s in ... select s.First => select expression must be MemberAccess");
    }
  }
}