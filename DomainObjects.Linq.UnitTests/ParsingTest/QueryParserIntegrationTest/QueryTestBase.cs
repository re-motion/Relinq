using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.Parsing;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.QueryParserIntegrationTest
{
  public abstract class QueryTestBase<T>
  {
    public Expression SourceExpression { get; private set; }
    public QueryExpression ParsedQuery { get; private set; }
    public IQueryable<Student> QuerySource { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
      QuerySource = ExpressionHelper.CreateQuerySource();
      SourceExpression = CreateQuery().Expression;
      QueryParser parser = new QueryParser (SourceExpression);
      ParsedQuery = parser.GetParsedQuery ();
    }

    protected abstract IQueryable<T> CreateQuery ();

    [Test]
    public void ParseResultIsNotNull()
    {
      Assert.IsNotNull (ParsedQuery);
    }

    [Test]
    public virtual void CheckFromClause ()
    {
      Assert.IsNotNull (ParsedQuery.FromClause);
      Assert.AreEqual ("s", ParsedQuery.FromClause.Identifier.Name);
      Assert.AreSame (typeof (Student), ParsedQuery.FromClause.Identifier.Type);
      Assert.AreSame (QuerySource, ParsedQuery.FromClause.QuerySource);
      Assert.AreEqual (0, ParsedQuery.FromClause.JoinClauseCount);
    }

    [Test]
    public void CheckQueryBody ()
    {
      Assert.IsNotNull (ParsedQuery.QueryBody);
    }

    [Test]
    public void OutputResult ()
    {
      Console.WriteLine (ParsedQuery);
    }

    public abstract void CheckFromLetWhereClauses ();
    public abstract void CheckOrderByClause ();
    public abstract void CheckSelectOrGroupClause ();
  }
}