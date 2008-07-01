using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using NUnit.Framework.SyntaxHelpers;

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
      _whereConditionParserRegistry = new WhereConditionParserRegistry (_databaseInfo);
      _parserRegistry = new ParserRegistry ();
    }

    [Test]
    public void Initialization_AddsDefaultParsers ()
    {
      WhereConditionParserRegistry whereConditionParserRegistry = 
        new WhereConditionParserRegistry (_databaseInfo);

      Assert.That (whereConditionParserRegistry.GetParsers (typeof (BinaryExpression)).ToArray (), Is.Not.Empty);
      Assert.That (whereConditionParserRegistry.GetParsers (typeof (ConstantExpression)).ToArray (), Is.Not.Empty);
      Assert.That (whereConditionParserRegistry.GetParsers (typeof (MemberExpression)).ToArray (), Is.Not.Empty);
      Assert.That (whereConditionParserRegistry.GetParsers (typeof (MethodCallExpression)).ToArray (), Is.Not.Empty);
      Assert.That (whereConditionParserRegistry.GetParsers (typeof (ParameterExpression)).ToArray (), Is.Not.Empty);
      Assert.That (whereConditionParserRegistry.GetParsers (typeof (UnaryExpression)).ToArray (), Is.Not.Empty);
      
    }

    [Test]
    public void RegisterNewMethodCallExpressionParser_RegisterFirst ()
    {
      Assert.That (_whereConditionParserRegistry.GetParsers (typeof (MethodCallExpression)).Count (), Is.EqualTo (3));
      
      LikeParser likeParser = new LikeParser (_whereConditionParserRegistry);
      _whereConditionParserRegistry.RegisterParser (typeof (MethodCallExpression), likeParser);
      Assert.That (_whereConditionParserRegistry.GetParsers (typeof (MethodCallExpression)).Count (), Is.EqualTo (4));
      Assert.That (_whereConditionParserRegistry.GetParsers (typeof (MethodCallExpression)).First (), Is.SameAs (likeParser));
    }
    

    [Test]
    public void GetParser ()
    {
      ConstantExpression constantExpression = Expression.Constant ("test");
      IWhereConditionParser expectedParser = _whereConditionParserRegistry.GetParsers (typeof (ConstantExpression)).First();

      Assert.AreSame (expectedParser, _whereConditionParserRegistry.GetParser (constantExpression));
    }
  }
}