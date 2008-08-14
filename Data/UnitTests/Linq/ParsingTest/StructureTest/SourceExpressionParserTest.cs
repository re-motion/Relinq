/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using System.Linq;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest
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
      ParseResultCollector result = Parse (query.Expression);
      var resultModifiers = result.ResultModifierData;
      Assert.IsEmpty (result.ResultModifierData);
    }

    [Test]
    public void Distinct_TopLevelBeforeSelect()
    {
      IQueryable<string> query = DistinctTestQueryGenerator.CreateSimpleDistinctQuery (_source);
      ParseResultCollector result = Parse (query.Expression);
      Assert.IsNotEmpty (result.ResultModifierData);
    }
    
    [Test]
    public void Distinct_TopLevelBeforeWhere ()
    {
      IQueryable<Student> query = DistinctTestQueryGenerator.CreateDisinctWithWhereQueryWithoutProjection (_source);
      ParseResultCollector result = Parse(query.Expression);
      Assert.IsNotNull (result.ResultModifierData);
    }

    [Test]
    public void ResultModifiers ()
    {
      var query = SelectTestQueryGenerator.CreateSimpleQuery (_source);
      var methodInfo = ParserUtility.GetMethod (() => query.Count ());
      MethodCallExpression countExpression = Expression.Call (methodInfo, query.Expression);
      ParseResultCollector expectedResult = Parse (countExpression);
      Assert.IsNotEmpty (expectedResult.ResultModifierData);
      Assert.That (expectedResult.ResultModifierData, Is.EqualTo (new[] {countExpression}));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Distinct is only allowed at the top level of a query, not in the middle: "
        + "'TestQueryable<Student>().Select(s => s).Distinct().Where(s => s.IsOld)'.")]
    public void Distinct_NonTopLevel ()
    {
      IQueryable<Student> query = _source.Select (s => s).Distinct().Where (s => s.IsOld);
      Parse (query.Expression);
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
    public void SimpleSource ()
    {
      ConstantExpression constantExpression = Expression.Constant (5);
      ParseResultCollector result = Parse (constantExpression);

      ParseResultCollector expectedResult = new ParseResultCollector (constantExpression);
      new SimpleFromSourceExpressionParser().Parse (expectedResult, constantExpression, _potentialFromIdentifier, "xy");
      AssertResultsEqual (expectedResult, result);

    }

    [Test]
    public void Select ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (_source);
      ParseResultCollector result = Parse (query.Expression);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new SelectExpressionParser().Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }
    
    [Test]
    public void SelectMany ()
    {
      IQueryable<Student> query = FromTestQueryGenerator.CreateMultiFromQuery (_source, _source);
      ParseResultCollector result = Parse (query.Expression);
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new SelectManyExpressionParser ().Parse (expectedResult, (MethodCallExpression) query.Expression);
      AssertResultsEqual (expectedResult, result);
    }

    [Test]
    public void Where_TopLevel ()
    {
      IQueryable<Student> query = WhereTestQueryGenerator.CreateSimpleWhereQuery(_source);
      ParseResultCollector result = Parse (query.Expression);
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
      ParseResultCollector result = Parse (query.Expression);
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

    [Test]
    public void Cast ()
    {
      IQueryable<object> query = CastTestQueryGenerator.CreateQueryWithTopLevelCastCall (_source);
      ParseResultCollector result = Parse (query.Expression);
      
      Expression innerExpression = ((MethodCallExpression)query.Expression).Arguments[0];
      ParseResultCollector expectedResult = new ParseResultCollector (query.Expression);
      new SourceExpressionParser (true).Parse (expectedResult, innerExpression, _potentialFromIdentifier, "whatever");
      AssertResultsEqual (expectedResult, result);
    }

    

    private void AssertResultsEqual(ParseResultCollector one, ParseResultCollector two)
    {
      Assert.AreEqual (one.ExpressionTreeRoot, two.ExpressionTreeRoot);
      //Assert.AreEqual (one.IsDistinct, two.IsDistinct);
      Assert.AreEqual (one.ResultModifierData, two.ResultModifierData);
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
          Assert.AreEqual (fromExpression1.TypedExpression, fromExpression2.TypedExpression);
        }
        else if (whereExpression1 != null)
        {
          WhereExpressionData whereExpression2 = two.BodyExpressions[i] as WhereExpressionData;
          Assert.AreEqual (whereExpression1.TypedExpression, whereExpression2.TypedExpression);
        }
        else if (letExpression1 != null)
        {
          LetExpressionData letExpression2 = two.BodyExpressions[i] as LetExpressionData;
          Assert.AreEqual (letExpression1.TypedExpression, letExpression2.TypedExpression);
        }
        else
        {
          OrderExpressionData orderExpression2 = two.BodyExpressions[i] as OrderExpressionData;
          Assert.AreEqual (orderExpression1.TypedExpression, orderExpression2.TypedExpression);
        }
      }
    }

    private ParseResultCollector Parse (Expression expression)
    {
      ParseResultCollector result = new ParseResultCollector (expression);
      _topLevelParser.Parse (result, expression, _potentialFromIdentifier, "bla");
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
