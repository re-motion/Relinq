using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class ConstantExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void Parse ()
    {
      ConstantExpression constantExpression = Expression.Constant (5);

      ConstantExpressionParser parser = new ConstantExpressionParser(StubDatabaseInfo.Instance);
      IEvaluation result = parser.Parse (constantExpression, ParseContext);

      //expected
      IEvaluation expected = new Constant (5);

      Assert.AreEqual (expected, result);
    }
  }
}