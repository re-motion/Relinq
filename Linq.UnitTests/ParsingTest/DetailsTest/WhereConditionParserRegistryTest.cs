using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest
{
  [TestFixture]
  public class WhereConditionParserRegistryTest
  {
    private IDatabaseInfo _databaseInfo;
    private ParameterExpression _parameter;
    private MainFromClause _fromClause;
    private QueryModel _queryModel;
    private JoinedTableContext _context;
    private WhereClause _whereClause;
    private WhereConditionParserRegistry _whereConditionParserRegistry;
    private ParserRegistry _parserRegistry;

    [SetUp]
    public void SetUp ()
    {
      _databaseInfo = StubDatabaseInfo.Instance;
      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = ExpressionHelper.CreateMainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      _queryModel = ExpressionHelper.CreateQueryModel (_fromClause);
      _context = new JoinedTableContext ();
      _whereClause = ExpressionHelper.CreateWhereClause ();
      _whereConditionParserRegistry = new WhereConditionParserRegistry (_queryModel, _databaseInfo, _context);
      _parserRegistry = new ParserRegistry ();
    }

    [Test]
    public void Initialization ()
    {
      WhereConditionParserRegistry whereConditionParserRegistry = 
        new WhereConditionParserRegistry (_queryModel, _databaseInfo, _context);
      
      Assert.IsNotNull (whereConditionParserRegistry.GetParsers<MethodCallExpression>());
    }

    [Test]
    public void GetAllRegisteredParserForMethodCallExpressionParser ()
    {
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, _parserRegistry);

      foreach (IWhereConditionParser<MethodCallExpression> parser in _whereConditionParserRegistry.GetParsers<MethodCallExpression>())
      {
        Assert.AreEqual (parser.GetType(), methodCallExpressionParser.GetType());
      }
    }

    [Test]
    public void RegisterNewMethodCallExpressionParser ()
    {
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, _parserRegistry);
      int cnt1 = 0;
      int cnt2 = 0;

      foreach (IWhereConditionParser<MethodCallExpression> parser in _whereConditionParserRegistry.GetParsers<MethodCallExpression> ())
      {
        cnt1++;
      }
      _whereConditionParserRegistry.RegisterParser (methodCallExpressionParser);
      foreach (IWhereConditionParser<MethodCallExpression> parser in _whereConditionParserRegistry.GetParsers<MethodCallExpression> ())
      {
        cnt2++;
      }

      Assert.AreEqual (1, cnt1);
      Assert.AreEqual (2, cnt2);
    }

    [Test]
    public void GetDefaultRegisteredConstantExpressionParser ()
    {
      ConstantExpression constantExpression = Expression.Constant("test");
      ConstantExpressionParser expectedParser = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      
      Assert.AreEqual(expectedParser.GetType(),_whereConditionParserRegistry.GetParser (constantExpression).GetType());
    }

    [Test]
    public void GetParser_NonGeneric ()
    {
      ConstantExpression constantExpression = Expression.Constant ("test");
      ConstantExpressionParser expectedParser = new ConstantExpressionParser (StubDatabaseInfo.Instance);

      Assert.AreEqual (expectedParser.GetType (), _whereConditionParserRegistry.GetParser (constantExpression).GetType ());
    }
  }
}