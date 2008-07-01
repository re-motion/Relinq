using System;
using System.Linq.Expressions;
using NUnit.Framework;
using System.Linq;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.StructureTest
{
  [TestFixture]
  public class SourceExpressionParserTest
  {
    private SourceExpressionParser _topLevelParser;
    private SourceExpressionParser _notTopLevelParser;
    private IQueryable<Student> _source;
    private ParameterExpression _potentialFromIdentifier;

    [SetUp]
    public void SetUp()
    {
      _topLevelParser = new SourceExpressionParser (true);
      _notTopLevelParser = new SourceExpressionParser (false);
      _source = ExpressionHelper.CreateQuerySource();
      _potentialFromIdentifier = ExpressionHelper.CreateParameterExpression();
    }

    [Test]
    public void NonDistinct ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (_source);
      ParseResultCollector result = Parse (query);
      Assert.IsFalse (result.IsDistinct);
    }

    [Test]
    public void Distinct_TopLevelBeforeSelect()
    {
      IQueryable<string> query = DistinctTestQueryGenerator.CreateSimpleDistinctQuery (_source);
      ParseResultCollector result = Parse (query);
      Assert.IsTrue (result.IsDistinct);
    }

    [Test]
    public void Distinct_TopLevelBeforeWhere ()
    {
      IQueryable<Student> query = DistinctTestQueryGenerator.CreateDisinctWithWhereQueryWithoutProjection (_source);
      ParseResultCollector result = Parse(query);
      Assert.IsTrue (result.IsDistinct);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Distinct is only allowed at the top level of a query, not in the middle: "
        + "'TestQueryable<Student>().Select(s => s).Distinct().Where(s => s.IsOld)'.")]
    public void Distinct_NonTopLevel ()
    {
      IQueryable<Student> query = _source.Select (s => s).Distinct().Where (s => s.IsOld);
      Parse (query);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected Constant or Call expression for xy, found i (ParameterExpression).")]
    public void InvalidSource ()
    {
      _topLevelParser.Parse (new ParseResultCollector (_source.Expression), _potentialFromIdentifier, _potentialFromIdentifier, "xy");
    }

    public static void Thrower()
    {
      throw new InvalidOperationException ("This method always throws.");
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "The expression 'Thrower()' could not be evaluated as a query source because it "
        + "threw an exception: This method always throws.")]
    public void Method_ThrowingException ()
    {
      Expression throwingCall = Expression.Call (typeof (SourceExpressionParserTest).GetMethod ("Thrower"));
      _topLevelParser.Parse (new ParseResultCollector (_source.Expression), throwingCall, _potentialFromIdentifier, "xy");
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "The expression 's.get_ID()' could not be evaluated as a query source because it "
        + "cannot be compiled: Lambda Parameter not in scope")]
    public void Method_ThrowingExceptionOnCompile ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      Expression throwingCall = Expression.Call (parameter, typeof (Student).GetMethod ("get_ID"));
      _topLevelParser.Parse (new ParseResultCollector (_source.Expression), throwingCall, _potentialFromIdentifier, "xy");
    }

    [Test]
    public void SimpleSource_Constant()
    {
      Expression constantExpression = Expression.Constant(ExpressionHelper.CreateQuerySource(), typeof (IQueryable<Student>));
      ParseResultCollector result = new ParseResultCollector (constantExpression);
      _topLevelParser.Parse (result, constantExpression, _potentialFromIdentifier, "bla");

      Assert.AreEqual (1, result.BodyExpressions.Count);
      Assert.AreEqual (_potentialFromIdentifier, ((FromExpressionData) result.BodyExpressions[0]).Identifier);
      Assert.AreEqual (constantExpression, ((FromExpressionData) result.BodyExpressions[0]).Expression);
      Assert.That (result.ProjectionExpressions, Is.Empty);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Query sources cannot be null.")]
    public void SimpleSource_ConstantNull ()
    {
      Expression constantExpression = Expression.Constant (null, typeof (IQueryable<Student>));
      ParseResultCollector result = new ParseResultCollector (constantExpression);
      _topLevelParser.Parse (result, constantExpression, _potentialFromIdentifier, "bla");
    }

    [Test]
    public void SimpleSource_MemberExpression ()
    {
      Expression memberExpression = Expression.MakeMemberAccess (Expression.Constant (null, typeof (Student)), typeof (Student).GetProperty ("Scores"));
      ParseResultCollector result = new ParseResultCollector(memberExpression);
      _topLevelParser.Parse (result, memberExpression, _potentialFromIdentifier, "bla");
      
      Assert.AreEqual (1, result.BodyExpressions.Count);
      Assert.AreEqual (_potentialFromIdentifier, ((FromExpressionData) result.BodyExpressions[0]).Identifier);
      Assert.AreEqual (memberExpression, ((FromExpressionData) result.BodyExpressions[0]).Expression);
      Assert.That (result.ProjectionExpressions, Is.Empty);
    }

    [Test]
    public void SimpleSource_MethodCall ()
    {
      Expression callExpression = Expression.Call (typeof (ExpressionHelper).GetMethod ("CreateQuerySource", new Type[0]));
      ParseResultCollector result = new ParseResultCollector (callExpression);
      _topLevelParser.Parse (result, callExpression, _potentialFromIdentifier, "bla");

      Assert.AreEqual (1, result.BodyExpressions.Count);
      Assert.AreEqual (_potentialFromIdentifier, ((FromExpressionData) result.BodyExpressions[0]).Identifier);
      Assert.IsInstanceOfType (typeof (TestQueryable<Student>), ((ConstantExpression)((FromExpressionData) result.BodyExpressions[0]).Expression).Value);
      Assert.That (result.ProjectionExpressions, Is.Empty);
    }

    [Test]
    public void Select ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (_source);
      ParseResultCollector result = Parse (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new SelectExpressionParser().Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void SelectMany ()
    {
      IQueryable<Student> query = FromTestQueryGenerator.CreateMultiFromQuery (_source, _source);
      ParseResultCollector result = Parse (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new SelectManyExpressionParser ().Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void Where_TopLevel ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateSimpleWhereQuery(_source);
      ParseResultCollector result = Parse (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new WhereExpressionParser (true).Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void Where_NotTopLevel ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateSimpleWhereQuery (_source);
      ParseResultCollector result = Parse_NotTopLevel (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new WhereExpressionParser (false).Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void OrderBy_TopLevel ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateSimpleOrderByQuery (_source);
      ParseResultCollector result = Parse (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new OrderByExpressionParser (true).Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void OrderBy_NotTopLevel ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateSimpleOrderByQuery (_source);
      ParseResultCollector result = Parse_NotTopLevel (query);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new OrderByExpressionParser (false).Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void SimpleLet ()
    {
      IQueryable<string> query = LetTestQueryGenerator.CreateSimpleLetClause (_source);

      Expression letExpression = ((MethodCallExpression) (query.Expression)).Arguments[0];
      ParseResultCollector result = new ParseResultCollector (letExpression);
      _notTopLevelParser.Parse (result, letExpression, _potentialFromIdentifier, "bla");

      ParseResultCollector expectedResult = new ParseResultCollector (letExpression);
      new LetExpressionParser ().Parse (expectedResult, (MethodCallExpression) letExpression);
      AssertResultsEqual (expectedResult, result);
    }

    private void AssertResultsEqual(ParseResultCollector one, ParseResultCollector two)
    {
      Assert.AreEqual (one.ExpressionTreeRoot, two.ExpressionTreeRoot);
      Assert.AreEqual (one.IsDistinct, two.IsDistinct);
      Assert.AreEqual (one.ProjectionExpressions.Count, two.ProjectionExpressions.Count);
      Assert.AreEqual (one.BodyExpressions.Count, two.BodyExpressions.Count);

      for (int i = 0; i < one.ProjectionExpressions.Count; ++i)
        Assert.AreEqual (one.ProjectionExpressions[i], two.ProjectionExpressions[i]);

      for (int i = 0; i < one.BodyExpressions.Count; ++i)
      {
        FromExpressionData fromExpression1 = one.BodyExpressions[i] as FromExpressionData;
        WhereExpressionData whereExpression1 = one.BodyExpressions[i] as WhereExpressionData;
        OrderExpressionData orderExpression1 = one.BodyExpressions[i] as OrderExpressionData;
        LetExpressionData letExpression1 = one.BodyExpressions[i] as LetExpressionData;
        if (fromExpression1 != null)
        {
          FromExpressionData fromExpression2 = two.BodyExpressions[i] as FromExpressionData;

          Assert.AreEqual (fromExpression1.Identifier, fromExpression2.Identifier);
          Assert.AreEqual (fromExpression1.Expression, fromExpression2.Expression);
        }
        else if (whereExpression1 != null)
        {
          WhereExpressionData whereExpression2 = two.BodyExpressions[i] as WhereExpressionData;
          Assert.AreEqual (whereExpression1.Expression, whereExpression2.Expression);
        }
        else if (letExpression1 != null)
        {
          LetExpressionData letExpression2 = two.BodyExpressions[i] as LetExpressionData;
          Assert.AreEqual (letExpression1.Expression, letExpression2.Expression);
        }
        else
        {
          OrderExpressionData orderExpression2 = two.BodyExpressions[i] as OrderExpressionData;
          Assert.AreEqual (orderExpression1.Expression, orderExpression2.Expression);
        }
      }
    }

    private ParseResultCollector Parse (IQueryable query)
    {
      ParseResultCollector result = new ParseResultCollector (query.Expression);
      _topLevelParser.Parse (result, query.Expression, _potentialFromIdentifier, "bla");
      return result;
    }

   
    private ParseResultCollector Parse_NotTopLevel (IQueryable query)
    {
      ParseResultCollector result = new ParseResultCollector (query.Expression);
      _notTopLevelParser.Parse (result, query.Expression, _potentialFromIdentifier, "bla");
      return result;
    }


  }
}