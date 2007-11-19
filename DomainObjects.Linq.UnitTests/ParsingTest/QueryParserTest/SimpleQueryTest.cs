using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using System.Linq;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.Parsing;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.QueryParserTest
{
  [TestFixture]
  public class SimpleQueryTest
  {
    private Expression _sourceExpression;
    private QueryExpression _parsedQuery;
    private IQueryable<Student> _querySource;

    [SetUp]
    public void SetUp()
    {
      _querySource = ExpressionHelper.CreateQuerySource();
      _sourceExpression = TestQueryGenerator.CreateSimpleQuery(_querySource).Expression;
      QueryParser parser = new QueryParser (_sourceExpression);
      _parsedQuery = parser.GetParsedQuery ();
    }

    [Test]
    public void ParseResultIsNotNull()
    {
      Assert.IsNotNull (_parsedQuery);
    }

    [Test]
    public void HasFromClause ()
    {
      Assert.IsNotNull (_parsedQuery.FromClause);
      Assert.AreEqual ("s", _parsedQuery.FromClause.Identifier.Name);
      Assert.AreSame (typeof (Student), _parsedQuery.FromClause.Identifier.Type);
      Assert.AreSame (_querySource, _parsedQuery.FromClause.QuerySource);
      Assert.AreEqual (0, _parsedQuery.FromClause.JoinClauseCount);
    }

    [Test]
    public void HasQueryBody()
    {
      Assert.IsNotNull (_parsedQuery.QueryBody);
      Assert.AreEqual (0, _parsedQuery.QueryBody.FromLetWhereClauseCount);
      Assert.IsNull (_parsedQuery.QueryBody.OrderByClause);
      Assert.IsNotNull (_parsedQuery.QueryBody.SelectOrGroupClause);
    }

    [Test]
    public void HasSelectClause()
    {
      SelectClause clause = _parsedQuery.QueryBody.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNotNull (clause.Expression);
      Assert.AreSame (_parsedQuery.FromClause.Identifier, clause.Expression,
          "from s in ... select s => select expression must be same as from-identifier"); 
    }

    [Test]
    public void OutputResult()
    {
      Console.WriteLine (_parsedQuery);
    }
  }
}