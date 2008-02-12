using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest
{
  [TestFixture]
  public class QueryParserTest
  {
    private Expression _expression;
    private QueryParser _parser;

    [SetUp]
    public void SetUp()
    {
      _expression = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource()).Expression;
      _parser = new QueryParser (_expression);
    }

    [Test]
    public void Initialize()
    {
      Assert.AreSame (_expression, _parser.SourceExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected one of 'Select, SelectMany, Where, OrderBy, OrderByDescending, ThenBy, ThenByDescending', but found 'WriteLine' at"
        + " position WriteLine() in tree WriteLine().")]
    public void Initialize_FromWrongExpression ()
    {
      MethodCallExpression expression = Expression.Call (typeof (Console), "WriteLine", Type.EmptyTypes);
      new QueryParser (expression);
    }

    [Test]
    public void GetParsedQuery()
    {
      Assert.IsNotNull (_parser.GetParsedQuery());
    }

    [Test]
    public void ParsedQuery_StoresExpressionTree ()
    {
      QueryExpression queryExpression = _parser.GetParsedQuery ();
      Assert.AreSame (_expression, queryExpression.GetExpressionTree());
    }

    [Test]
    public void PreviousClauses_SimpleQuery()
    {
      QueryExpression parsedQuery = _parser.GetParsedQuery();
      Assert.IsNull (parsedQuery.MainFromClause.PreviousClause);
      Assert.AreSame (parsedQuery.MainFromClause, parsedQuery.QueryBody.SelectOrGroupClause.PreviousClause);
    }

    [Test]
    public void PreviousClauses_LargeQuery ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource();
      Expression queryExpression = TestQueryGenerator.CreateMultiFromWhereQuery (source, source).Expression;
      QueryParser parser = new QueryParser (queryExpression);
      QueryExpression parsedQuery = parser.GetParsedQuery ();
      
      Assert.IsNull (parsedQuery.MainFromClause.PreviousClause);
      Assert.AreSame (parsedQuery.QueryBody.BodyClauses.First(), parsedQuery.QueryBody.BodyClauses.Last ().PreviousClause);
      Assert.AreSame (parsedQuery.QueryBody.BodyClauses.Last(), parsedQuery.QueryBody.SelectOrGroupClause.PreviousClause);
    }

    [Test]
    public void PreviousClauses_MultiFromWhereOrderByQuery()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource();
      Expression queryExpression = TestQueryGenerator.CreateMultiFromWhereOrderByQuery (source, source).Expression;
      QueryParser parser = new QueryParser (queryExpression);
      QueryExpression parsedQuery = parser.GetParsedQuery();

      Assert.IsNull (parsedQuery.MainFromClause.PreviousClause);
      Assert.AreSame (parsedQuery.QueryBody.BodyClauses.First(), parsedQuery.QueryBody.BodyClauses.Skip(1).First().PreviousClause);
      Assert.AreSame (parsedQuery.QueryBody.BodyClauses.Skip (1).First (), parsedQuery.QueryBody.BodyClauses.Last().PreviousClause);
    }






  }
}