using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest
{
  [TestFixture][Ignore("TODO: implement parsing of simple where clauses")]
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
      Assert.IsNotNull (whereClause.BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClause.BoolExpression.Type);
      Assert.AreSame (ParsedQuery.FromClause.Identifier, ((LambdaExpression) whereClause.BoolExpression).Parameters[0]);

    }
  }
}