using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class MultiWhereQueryTest : QueryTestBase<Student>
  {
    
    protected override IQueryable<Student> CreateQuery ()
    {
      return TestQueryGenerator.CreateMultiWhereQuery (QuerySource);
    }

    [Test]
    public override void CheckBodyClause ()
    {
      Assert.AreEqual (3, ParsedQuery.QueryBody.BodyClauseCount);
      Assert.IsNotNull (ParsedQuery.QueryBody.BodyClauses);
      WhereClause[] whereClauses = ParsedQuery.QueryBody.BodyClauses.Cast<WhereClause>().ToArray();

      ExpressionTreeNavigator navigator = new ExpressionTreeNavigator (whereClauses[0].BoolExpression);

      Assert.IsNotNull (whereClauses[0].BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClauses[0].BoolExpression);
      Assert.AreSame (ParsedQuery.FromClause.Identifier, navigator.Parameters[0].Expression);
      Assert.IsInstanceOfType (typeof (BinaryExpression), whereClauses[0].BoolExpression.Body);
      Assert.AreEqual ("Garcia", navigator.Body.Right.Value);

      navigator = new ExpressionTreeNavigator (whereClauses[1].BoolExpression);

      Assert.IsNotNull (whereClauses[1].BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClauses[1].BoolExpression);
      Assert.IsInstanceOfType (typeof (BinaryExpression), whereClauses[1].BoolExpression.Body);
      Assert.AreEqual ("Hugo", navigator.Body.Right.Value);

      navigator = new ExpressionTreeNavigator (whereClauses[2].BoolExpression);

      Assert.IsNotNull (whereClauses[2].BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClauses[2].BoolExpression);
      Assert.IsInstanceOfType (typeof (BinaryExpression), whereClauses[2].BoolExpression.Body);
      Assert.AreEqual (100, navigator.Body.Right.Value);
    }

    

    [Test]
    public override void CheckSelectOrGroupClause ()
    {
      Assert.IsNotNull (ParsedQuery.QueryBody.SelectOrGroupClause);
      SelectClause clause = ParsedQuery.QueryBody.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNull (clause.ProjectionExpression);
    }
  }
}