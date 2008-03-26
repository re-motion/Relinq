using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Parsing.Structure;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest.QueryParserIntegrationTest
{
  public abstract class QueryTestBase<T>
  {
    public Expression SourceExpression { get; private set; }
    public ExpressionTreeNavigator SourceExpressionNavigator { get; private set; }
    public QueryExpression ParsedQuery { get; private set; }
    public IQueryable<Student> QuerySource { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
      QuerySource = ExpressionHelper.CreateQuerySource();
      SourceExpression = CreateQuery().Expression;
      SourceExpressionNavigator = new ExpressionTreeNavigator (SourceExpression);
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
    public virtual void CheckMainFromClause ()
    {
      Assert.IsNotNull (ParsedQuery.MainFromClause);
      Assert.AreEqual ("s", ParsedQuery.MainFromClause.Identifier.Name);
      Assert.AreSame (typeof (Student), ParsedQuery.MainFromClause.Identifier.Type);
      Assert.AreSame (QuerySource, ParsedQuery.MainFromClause.QuerySource);
      Assert.AreEqual (0, ParsedQuery.MainFromClause.JoinClauses.Count);
    }
    
    [Test]
    public void OutputResult ()
    {
      Console.WriteLine (ParsedQuery);
    }

    [Test]
    public void TranslateBack()
    {
      Expression builtExpressionTree = ParsedQuery.GetExpressionTree();
      ExpressionTreeComparer.CheckAreEqualTrees (builtExpressionTree, SourceExpression);
    }

    public abstract void CheckBodyClause ();
    public abstract void CheckSelectOrGroupClause ();
  }
}