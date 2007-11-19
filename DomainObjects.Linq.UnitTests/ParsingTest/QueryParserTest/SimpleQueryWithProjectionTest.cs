using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.QueryParserTest
{
  [TestFixture]
  public class SimpleQueryWithProjectionTest: QueryTestBase<string>
  {
    protected override IQueryable<string> CreateQuery ()
    {
      return TestQueryGenerator.CreateSimpleQueryWithProjection (QuerySource);
    }

    [Test]
    public void ParseResultIsNotNull ()
    {
      Assert.IsNotNull (ParsedQuery);
    }

    [Test]
    public void HasFromClause ()
    {
      Assert.IsNotNull (ParsedQuery.FromClause);
      Assert.AreEqual ("s", ParsedQuery.FromClause.Identifier.Name);
      Assert.AreSame (typeof (Student), ParsedQuery.FromClause.Identifier.Type);
      Assert.AreSame (QuerySource, ParsedQuery.FromClause.QuerySource);
      Assert.AreEqual (0, ParsedQuery.FromClause.JoinClauseCount);
    }

    [Test]
    public void HasQueryBody ()
    {
      Assert.IsNotNull (ParsedQuery.QueryBody);
      Assert.AreEqual (0, ParsedQuery.QueryBody.FromLetWhereClauseCount);
      Assert.IsNull (ParsedQuery.QueryBody.OrderByClause);
      Assert.IsNotNull (ParsedQuery.QueryBody.SelectOrGroupClause);
    }

    [Test]
    public void HasSelectClause ()
    {
      SelectClause clause = ParsedQuery.QueryBody.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNotNull (clause.Expression);
      Assert.IsInstanceOfType (typeof(MemberExpression),clause.Expression,
          "from s in ... select s.First => select expression must be MemberAccess");
    }

    [Test]
    public void OutputResult ()
    {
      Console.WriteLine (ParsedQuery);
    }
  
    
  }
}