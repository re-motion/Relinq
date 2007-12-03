using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class MultiFromWhereQueryTest : QueryTestBase<Student>
  {
    private IQueryable<Student> _querySource2;

    public override void SetUp ()
    {
      _querySource2 = ExpressionHelper.CreateQuerySource();
      base.SetUp ();
    }

    protected override IQueryable<Student> CreateQuery ()
    {
      return TestQueryGenerator.CreateMultiFromWhereQuery (QuerySource,_querySource2);
    }

    [Test]
    public override void CheckMainFromClause ()
    {
      Assert.IsNotNull (ParsedQuery.FromClause);
      Assert.AreEqual ("s1", ParsedQuery.FromClause.Identifier.Name);
      Assert.AreSame (typeof (Student), ParsedQuery.FromClause.Identifier.Type);
      Assert.AreSame (QuerySource, ParsedQuery.FromClause.QuerySource);
      Assert.AreEqual (0, ParsedQuery.FromClause.JoinClauseCount);
    }

    [Test]
    public override void CheckFromLetWhereClauses ()
    {
      Assert.AreEqual (2, ParsedQuery.QueryBody.FromLetWhereClauseCount);
      AdditionalFromClause fromClause = ParsedQuery.QueryBody.FromLetWhereClauses.First() as AdditionalFromClause;
      Assert.IsNotNull (fromClause);
      Assert.AreSame (SourceExpressionNavigator.Arguments[0].Arguments[0].Arguments[1].Operand.Expression, fromClause.FromExpression);
      Assert.AreSame (SourceExpressionNavigator.Arguments[0].Arguments[0].Arguments[2].Operand.Expression, fromClause.ProjectionExpression);

      WhereClause whereClause = ParsedQuery.QueryBody.FromLetWhereClauses.Last() as WhereClause;
      Assert.IsNotNull (whereClause);
      Assert.AreSame (SourceExpressionNavigator.Arguments[0].Arguments[1].Operand.Expression, whereClause.BoolExpression);
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
      
      Assert.AreSame (SourceExpressionNavigator.Arguments[1].Operand.Expression, clause.ProjectionExpression);
    }
  }
}