using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Data.Linq.UnitTests.TestQueryGenerators;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest
{
  [TestFixture]
  public class QueryParserTest
  {
    private Expression _expression;
    private QueryParser _parser;

    [SetUp]
    public void SetUp()
    {
      _expression = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource()).Expression;
      _parser = new QueryParser (_expression);
    }

    [Test]
    public void Initialize()
    {
      Assert.AreSame (_expression, _parser.SourceExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected one of 'Select, SelectMany, Where, OrderBy, OrderByDescending, ThenBy, ThenByDescending, Distinct', but found 'WriteLine' at"
        + " position WriteLine() in tree WriteLine().")]
    public void Initialize_FromWrongExpression ()
    {
      MethodCallExpression expression = Expression.Call (typeof (Console), "WriteLine", Type.EmptyTypes);
      new QueryParser (expression).GetParsedQuery();
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
      Assert.AreSame (parsedQuery.MainFromClause, parsedQuery.SelectOrGroupClause.PreviousClause);
    }

    [Test]
    public void PreviousClauses_LargeQuery ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource();
      Expression queryExpression = MixedTestQueryGenerator.CreateMultiFromWhereQuery (source, source).Expression;
      QueryParser parser = new QueryParser (queryExpression);
      QueryExpression parsedQuery = parser.GetParsedQuery ();
      
      Assert.IsNull (parsedQuery.MainFromClause.PreviousClause);
      Assert.AreSame (parsedQuery.BodyClauses.First(), parsedQuery.BodyClauses.Last ().PreviousClause);
      Assert.AreSame (parsedQuery.BodyClauses.Last(), parsedQuery.SelectOrGroupClause.PreviousClause);
    }

    [Test]
    public void PreviousClauses_MultiFromWhereOrderByQuery()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource();
      Expression queryExpression = MixedTestQueryGenerator.CreateMultiFromWhereOrderByQuery (source, source).Expression;
      QueryParser parser = new QueryParser (queryExpression);
      QueryExpression parsedQuery = parser.GetParsedQuery();

      Assert.IsNull (parsedQuery.MainFromClause.PreviousClause);
      Assert.AreSame (parsedQuery.BodyClauses.First(), parsedQuery.BodyClauses.Skip(1).First().PreviousClause);
      Assert.AreSame (parsedQuery.BodyClauses.Skip (1).First (), parsedQuery.BodyClauses.Last().PreviousClause);
    }






  }
}