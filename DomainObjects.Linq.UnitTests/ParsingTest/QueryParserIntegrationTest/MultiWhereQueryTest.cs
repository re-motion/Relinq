using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class MultiWhereQueryTest : QueryTestBase<Student>
  {
    
    protected override IQueryable<Student> CreateQuery ()
    {
      return TestQueryGenerator.CreateMultiWhereQuery (QuerySource);
    }

    [Test]
    public override void CheckFromLetWhereClauses ()
    {
      Assert.AreEqual (3, ParsedQuery.QueryBody.FromLetWhereClauseCount);
      Assert.IsNotNull (ParsedQuery.QueryBody.FromLetWhereClauses);
      WhereClause[] whereClauses = ParsedQuery.QueryBody.FromLetWhereClauses.Cast<WhereClause>().ToArray();
      
      Assert.IsNotNull (whereClauses[0].BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClauses[0].BoolExpression);
      Assert.AreSame (ParsedQuery.FromClause.Identifier, ((LambdaExpression) whereClauses[0].BoolExpression).Parameters[0]);
      Assert.IsInstanceOfType (typeof (BinaryExpression), whereClauses[0].BoolExpression.Body);
      Assert.AreEqual ("Garcia", ((ConstantExpression) ((BinaryExpression) whereClauses[0].BoolExpression.Body).Right).Value);

      Assert.IsNotNull (whereClauses[1].BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClauses[1].BoolExpression);
      Assert.IsInstanceOfType (typeof (BinaryExpression), whereClauses[1].BoolExpression.Body);
      Assert.AreEqual ("Hugo", ((ConstantExpression) ((BinaryExpression) whereClauses[1].BoolExpression.Body).Right).Value);

      Assert.IsNotNull (whereClauses[2].BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClauses[2].BoolExpression);
      Assert.IsInstanceOfType (typeof (BinaryExpression), whereClauses[2].BoolExpression.Body);
      Assert.AreEqual (100, ((ConstantExpression) ((BinaryExpression) whereClauses[2].BoolExpression.Body).Right).Value);
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
      Assert.AreSame (ParsedQuery.FromClause.Identifier, clause.ProjectionExpressions[0].Body,
                      "from s in ... select s => select expression must be same as from-identifier");
    }
  }
}