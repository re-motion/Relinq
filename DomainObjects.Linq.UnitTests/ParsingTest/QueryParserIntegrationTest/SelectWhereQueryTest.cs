using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class SelectWhereQueryTest : QueryTestBase<string>
  {
    protected override IQueryable<string> CreateQuery ()
    {
      return TestQueryGenerator.CreateSelectWhereQuery (QuerySource);
    }

    [Test]
    public override void CheckFromLetWhereClauses ()
    {
      Assert.AreEqual (1, ParsedQuery.QueryBody.FromLetWhereClauseCount);
      WhereClause whereClause = ParsedQuery.QueryBody.FromLetWhereClauses.First () as WhereClause;
      Assert.IsNotNull (whereClause);
      Assert.AreSame (((UnaryExpression)((MethodCallExpression)((MethodCallExpression) SourceExpression).Arguments[0]).Arguments[1]).Operand,
          whereClause.BoolExpression);
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
      Assert.IsInstanceOfType (typeof (MemberExpression), clause.ProjectionExpressions[0].Body,
                      "from s in ... select s.First => select expression must be member access");
      Assert.AreEqual ("First", ((MemberExpression)clause.ProjectionExpressions[0].Body).Member.Name,
                      "from s in ... select s.First => select expression must be access to First member");
    }
  }
}