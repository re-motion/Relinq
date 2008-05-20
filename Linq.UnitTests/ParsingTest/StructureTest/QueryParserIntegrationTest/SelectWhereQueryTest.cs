using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.StructureTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class SelectWhereQueryTest : QueryTestBase<string>
  {
    protected override IQueryable<string> CreateQuery ()
    {
      return WhereTestQueryGenerator.CreateSelectWhereQuery (QuerySource);
    }

    [Test]
    public override void CheckBodyClauses ()
    {
      Assert.AreEqual (1, ParsedQuery.BodyClauses.Count);
      WhereClause whereClause = ParsedQuery.BodyClauses.First () as WhereClause;
      Assert.IsNotNull (whereClause);

      Assert.AreSame (SourceExpressionNavigator.Arguments[0].Arguments[1].Operand.Expression, whereClause.BoolExpression);
    }

    [Test]
    public override void CheckSelectOrGroupClause ()
    {
      Assert.IsNotNull (ParsedQuery.SelectOrGroupClause);
      SelectClause clause = ParsedQuery.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNotNull (clause.ProjectionExpression);
      Assert.IsInstanceOfType (typeof (MemberExpression), clause.ProjectionExpression.Body,
          "from s in ... select s.First => select expression must be member access");
      Assert.AreEqual ("First", ((MemberExpression) clause.ProjectionExpression.Body).Member.Name,
          "from s in ... select s.First => select expression must be access to First member");
    }
  }
}