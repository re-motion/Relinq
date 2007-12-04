using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Clauses;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class SimpleWhereQueryTest : SimpleQueryTest
  {
    protected override System.Linq.IQueryable<Student> CreateQuery ()
    {
      return TestQueryGenerator.CreateSimpleWhereQuery(QuerySource);
    }

    [Test]
    public override void CheckFromLetWhereClauses ()
    {
      Assert.AreEqual (1, ParsedQuery.QueryBody.FromLetWhereClauseCount);
      WhereClause whereClause = ParsedQuery.QueryBody.FromLetWhereClauses.First() as WhereClause;
      Assert.IsNotNull (whereClause);

      ExpressionTreeNavigator navigator = new ExpressionTreeNavigator (whereClause.BoolExpression);
      Assert.IsNotNull (whereClause.BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClause.BoolExpression);
      Assert.AreSame (ParsedQuery.FromClause.Identifier, navigator.Parameters[0].Expression);
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