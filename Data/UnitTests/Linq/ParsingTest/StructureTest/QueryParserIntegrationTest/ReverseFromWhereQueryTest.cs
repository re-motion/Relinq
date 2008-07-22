using System;
using System.Linq;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class ReverseFromWhereQueryTest : QueryTestBase<Student>
  {
    private IQueryable<Student> _querySource2;

    public override void SetUp ()
    {
      _querySource2 = ExpressionHelper.CreateQuerySource();
      base.SetUp ();
    }

    protected override IQueryable<Student> CreateQuery ()
    {
      return MixedTestQueryGenerator.CreateReverseFromWhereQuery (QuerySource, _querySource2);
    }

    [Test]
    public override void CheckMainFromClause ()
    {
      Assert.IsNotNull (ParsedQuery.MainFromClause);
      Assert.AreEqual ("s1", ParsedQuery.MainFromClause.Identifier.Name);
      Assert.AreSame (typeof (Student), ParsedQuery.MainFromClause.Identifier.Type);
      ExpressionTreeComparer.CheckAreEqualTrees (QuerySourceExpression, ParsedQuery.MainFromClause.QuerySource);
      Assert.AreEqual (0, ParsedQuery.MainFromClause.JoinClauses.Count);
    }

    [Test]
    public override void CheckBodyClauses ()
    {
      Assert.AreEqual (2, ParsedQuery.BodyClauses.Count);
            
      WhereClause whereClause = ParsedQuery.BodyClauses.First() as WhereClause;
      Assert.IsNotNull (whereClause);
      Assert.AreSame (SourceExpressionNavigator.Arguments[0].Arguments[1].Operand.Expression, whereClause.BoolExpression);

      AdditionalFromClause fromClause = ParsedQuery.BodyClauses.Last () as AdditionalFromClause;
      Assert.IsNotNull (fromClause);
      Assert.AreSame (SourceExpressionNavigator.Arguments[1].Operand.Expression, fromClause.FromExpression);
      Assert.AreSame (SourceExpressionNavigator.Arguments[2].Operand.Expression, fromClause.ProjectionExpression);
    }

    
    [Test]
    public override void CheckSelectOrGroupClause ()
    {
      Assert.IsNotNull (ParsedQuery.SelectOrGroupClause);
      SelectClause clause = ParsedQuery.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNotNull (clause.ProjectionExpression);

      Assert.AreSame (SourceExpressionNavigator.Arguments[2].Operand.Expression, clause.ProjectionExpression);
    }
  }
}