using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest
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
    [ExpectedException (typeof (QueryParserException), ExpectedMessage = "Expected one of 'Select, Where', but found 'WriteLine' at position "
        + "WriteLine() in tree WriteLine().")]
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
  }
}