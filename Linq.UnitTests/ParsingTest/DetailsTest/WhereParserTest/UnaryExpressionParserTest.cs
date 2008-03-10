using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.WhereParser;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereParserTest
{
  [TestFixture]
  public class UnaryExpressionParserTest
  {
    [Test]
    public void Parse()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause();
      UnaryExpression unaryExpression = Expression.Not (Expression.Constant (5));

      ICriterion criterion = new Constant (5);
      ICriterion expectedCriterion = new NotCriterion (new Constant (5));

      UnaryExpressionParser parser = new UnaryExpressionParser (whereClause, delegate (Expression expression)
      {
        Expression.Constant (5);
        return criterion;
      });

      ICriterion actualCriterion = parser.Parse (unaryExpression);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }
  }
}