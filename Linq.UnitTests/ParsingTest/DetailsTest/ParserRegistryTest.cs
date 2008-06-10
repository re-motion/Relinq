using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Utilities;


namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest
{
  [TestFixture]
  public class ParserRegistryTest
  {
    private ParameterExpression _parameter;
    private IColumnSource _fromSource;
    private MainFromClause _fromClause;
    private QueryModel _queryModel;


    [SetUp]
    public void SetUp ()
    {
      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = ExpressionHelper.CreateMainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      _fromSource = _fromClause.GetFromSource (StubDatabaseInfo.Instance);
      _queryModel = ExpressionHelper.CreateQueryModel (_fromClause);
      Column column = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column };
    }

    [Test]
    [Ignore ("TODO: Check higher priority")]
    public void RegisterParser ()
    {
      ParserRegistry parserRegistry = new ParserRegistry ();

      Column column = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column };

      ICriterion criterion = new Column (new Table ("Student", "s"), "First");
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, parserRegistry);
      parserRegistry.RegisterParser (methodCallExpressionParser);

      ConstantExpressionParser constantExpressionParser = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser(constantExpressionParser); 
    }

    [Test]
    public void GetAllRegisteredParserForMethodCallExpressionParser ()
    {
      ParserRegistry parserRegistry = new ParserRegistry ();
      Column column = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column };

      ICriterion criterion = new Column (new Table ("Student", "s"), "First");
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, parserRegistry);

      parserRegistry.RegisterParser (methodCallExpressionParser);
      parserRegistry.RegisterParser (new ConstantExpressionParser (StubDatabaseInfo.Instance));

      foreach (IParser<MethodCallExpression> parser in parserRegistry.GetParsers<MethodCallExpression> ())
      {
        Assert.AreEqual (parser, methodCallExpressionParser);
      }
    }

    [Test]
    public void GetAllRegisteresParser_NoParserRegistered ()
    {
      ParserRegistry parserRegistry = new ParserRegistry ();
      IEnumerable resultList = (parserRegistry.GetParsers<Expression> ());
      Assert.IsFalse (resultList.GetEnumerator ().MoveNext ());
    }

    [Test]
    public void GetParser_FirstRegisteredParser ()
    {
      ParserRegistry parserRegistry = new ParserRegistry ();

      Column column = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column };

      ICriterion criterion = new Column (new Table ("Student", "s"), "First");
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, parserRegistry);

      parserRegistry.RegisterParser (methodCallExpressionParser);
      ConstantExpressionParser expectedParser = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (expectedParser);

      ConstantExpression constantExpression = Expression.Constant (5);
      IParser<ConstantExpression> actualParser = parserRegistry.GetParser (constantExpression);

      Assert.AreEqual (expectedParser, actualParser);
    }

    [Test]
    [ExpectedException (typeof (ParseException), ExpectedMessage = "Cannot parse Constant, no appropriate parser found")]
    public void GetParser_NoParserFound ()
    {
      ParserRegistry parserRegistry = new ParserRegistry ();

      Column column = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column };

      ICriterion criterion = new Column (new Table ("Student", "s"), "First");
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, parserRegistry);

      parserRegistry.RegisterParser (methodCallExpressionParser);

      ConstantExpression constantExpression = Expression.Constant (5);
      IParser<ConstantExpression> actualParser = parserRegistry.GetParser (constantExpression);
    }

    [Test]
    public void GetParser_NonGeneric ()
    {
      ParserRegistry parserRegistry = new ParserRegistry ();
      Column column = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column };

      ICriterion criterion = new Column (new Table ("Student", "s"), "First");
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, parserRegistry);

      parserRegistry.RegisterParser (methodCallExpressionParser);
      ConstantExpressionParser expectedParser = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (expectedParser);

      ConstantExpression constantExpression = Expression.Constant (5);
      IParser actualParser = (IParser) parserRegistry.GetParser (constantExpression);

      Assert.AreEqual (expectedParser, actualParser);
    }
  }
}