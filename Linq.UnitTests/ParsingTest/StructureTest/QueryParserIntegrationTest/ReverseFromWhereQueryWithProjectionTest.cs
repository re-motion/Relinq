using System;
using System.Linq;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class ReverseFromWhereQueryWithProjectionTest : QueryTestBase<string>
  {
    private IQueryable<Student> _querySource2;

    public override void SetUp ()
    {
      _querySource2 = ExpressionHelper.CreateQuerySource();
      base.SetUp ();
    }

    protected override IQueryable<string> CreateQuery ()
    {
      return TestQueryGenerator.CreateReverseFromWhereQueryWithProjection (QuerySource,_querySource2);
    }

    [Test]
    public override void CheckMainFromClause ()
    {
      Assert.IsNotNull (ParsedQuery.MainFromClause);
      Assert.AreEqual ("s1", ParsedQuery.MainFromClause.Identifier.Name);
      Assert.AreSame (typeof (Student), ParsedQuery.MainFromClause.Identifier.Type);
      Assert.AreSame (QuerySource, ParsedQuery.MainFromClause.QuerySource);
      Assert.AreEqual (0, ParsedQuery.MainFromClause.JoinClauses.Count);
    }

    [Test]
    public override void CheckBodyClause ()
    {
      Assert.AreEqual (2, ParsedQuery.QueryBody.BodyClauses.Count);
            
      WhereClause whereClause = ParsedQuery.QueryBody.BodyClauses.First() as WhereClause;
      Assert.IsNotNull (whereClause);
      Assert.AreSame (SourceExpressionNavigator.Arguments[0].Arguments[1].Operand.Expression, whereClause.BoolExpression);

      AdditionalFromClause fromClause = ParsedQuery.QueryBody.BodyClauses.Last () as AdditionalFromClause;
      Assert.IsNotNull (fromClause);
      Assert.AreSame (SourceExpressionNavigator.Arguments[1].Operand.Expression, fromClause.FromExpression);
      Assert.AreSame (SourceExpressionNavigator.Arguments[2].Operand.Expression, fromClause.ProjectionExpression);

    }

    [Test]
    public override void CheckSelectOrGroupClause ()
    {
      Assert.IsNotNull (ParsedQuery.QueryBody.SelectOrGroupClause);
      SelectClause clause = ParsedQuery.QueryBody.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNotNull (clause.ProjectionExpression);

      Assert.AreSame (SourceExpressionNavigator.Arguments[2].Operand.Expression, clause.ProjectionExpression);
    }
  }
}