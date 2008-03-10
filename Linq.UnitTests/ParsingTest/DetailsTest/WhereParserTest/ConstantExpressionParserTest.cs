using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.WhereParser;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereParserTest
{
  [TestFixture]
  public class ConstantExpressionParserTest
  {
    [Test]
    public void Parse()
    {
      object expected = new Constant (5);
      ConstantExpressionParser parser = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      object result = parser.Parse (Expression.Constant(5, typeof (int)));
      Assert.AreEqual (expected, result);
    }
  }
}