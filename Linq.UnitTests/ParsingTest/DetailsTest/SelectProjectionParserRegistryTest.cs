using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest
{
  [TestFixture]
  public class SelectProjectionParserRegistryTest
  {
    private IDatabaseInfo _databaseInfo;
    private ParameterExpression _parameter;
    private MainFromClause _fromClause;
    private QueryModel _queryModel;
    private JoinedTableContext _context;
    private SelectProjectionParserRegistry _selectProjectionParserRegistry;
    private ParserRegistry _parserRegistry;
    private ParseContext _parseContext;

    [SetUp]
    public void SetUp ()
    {
      _databaseInfo = StubDatabaseInfo.Instance;
      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = ExpressionHelper.CreateMainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      _queryModel = ExpressionHelper.CreateQueryModel (_fromClause);
      _context = new JoinedTableContext ();
      _parseContext = new ParseContext();
      _selectProjectionParserRegistry = new SelectProjectionParserRegistry (_queryModel, _databaseInfo, _context, _parseContext);
      _parserRegistry = new ParserRegistry ();
    }

    [Test]
    public void Initialization ()
    {
      SelectProjectionParserRegistry selectProjectionParserRegistry =
        new SelectProjectionParserRegistry (_queryModel, _databaseInfo, _context,_parseContext);

      Assert.IsNotNull (selectProjectionParserRegistry.GetParsers<MethodCallExpression> ());
    }

    [Test]
    public void GetAllRegisteredParserForMethodCallExpressionParser ()
    {
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, _parserRegistry);

      foreach (ISelectProjectionParser<MethodCallExpression> parser in _selectProjectionParserRegistry.GetParsers<MethodCallExpression> ())
      {
        Assert.AreEqual (parser.GetType (), methodCallExpressionParser.GetType ());
      }
    }

    [Test]
    public void RegisterNewMethodCallExpressionParser ()
    {
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, _parserRegistry);
      int cnt1 = 0;
      int cnt2 = 0;

      foreach (ISelectProjectionParser<MethodCallExpression> parser in _selectProjectionParserRegistry.GetParsers<MethodCallExpression> ())
      {
        cnt1++;
      }
      _selectProjectionParserRegistry.RegisterParser (methodCallExpressionParser);
      foreach (ISelectProjectionParser<MethodCallExpression> parser in _selectProjectionParserRegistry.GetParsers<MethodCallExpression> ())
      {
        cnt2++;
      }

      Assert.AreEqual (1, cnt1);
      Assert.AreEqual (2, cnt2);
    }

    [Test]
    public void GetDefaultRegisteredConstantExpressionParser ()
    {
      ConstantExpression constantExpression = Expression.Constant ("test");
      ConstantExpressionParser expectedParser = new ConstantExpressionParser (StubDatabaseInfo.Instance);

      Assert.AreEqual (expectedParser.GetType (), _selectProjectionParserRegistry.GetParser (constantExpression).GetType ());
    }

    [Test]
    public void GetParser_NonGeneric ()
    {
      ConstantExpression constantExpression = Expression.Constant ("test");
      ConstantExpressionParser expectedParser = new ConstantExpressionParser (StubDatabaseInfo.Instance);

      Assert.AreEqual (expectedParser.GetType (), _selectProjectionParserRegistry.GetParser (constantExpression).GetType ());
    }


  }
}