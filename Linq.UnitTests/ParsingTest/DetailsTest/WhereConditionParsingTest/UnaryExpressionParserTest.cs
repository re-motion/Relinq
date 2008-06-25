using System.Linq.Expressions;
using NUnit.Framework;
using System.Collections.Generic;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class UnaryExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void Parse ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel();
      UnaryExpression unaryExpression = Expression.Not (Expression.Constant (5));
      ICriterion expectedCriterion = new NotCriterion (new Constant (5));

      WhereConditionParserRegistry parserRegistry = 
        new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof(ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));

      UnaryExpressionParser parser = new UnaryExpressionParser(parserRegistry);

      ICriterion actualCriterion = parser.Parse (unaryExpression, ParseContext);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }
  }
}