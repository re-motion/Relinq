using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.UnitTests.ParsingTest.StructureTest.WhereExpressionParserTest;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.StructureTest.LetExpressionParserTest
{
  [TestFixture]
  public class SimpleLetParserTest
  {
    private MethodCallExpression _letExpression;
    private ParseResultCollector _result;
    private BodyHelper _bodyHelper;

    [SetUp]
    public void SetUp ()
    {
      _letExpression = (MethodCallExpression) LetTestQueryGenerator.CreateSimpleSelect_LetExpression (ExpressionHelper.CreateQuerySource ()).Arguments[0];
      _result = new ParseResultCollector (_letExpression);
      new LetExpressionParser ().Parse (_result, _letExpression);
      _bodyHelper = new BodyHelper (_result.BodyExpressions);
    }

    [Test]
    public void ParsesLetExpression ()
    {
      Assert.IsNotNull (_bodyHelper.LetExpressions);
    }

    [Test]
    public void ParsesLetIdentifiers ()
    {
      Assert.IsNotNull (_bodyHelper.LetIdentifiers);
      Assert.IsInstanceOfType (typeof (ParameterExpression), _bodyHelper.LetIdentifiers[0]);
      Assert.AreEqual ("x", _bodyHelper.LetIdentifiers[0].Name);
      Assert.AreEqual (typeof(string), _bodyHelper.LetIdentifiers[0].Type);
    }
  }
}