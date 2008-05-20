using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
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