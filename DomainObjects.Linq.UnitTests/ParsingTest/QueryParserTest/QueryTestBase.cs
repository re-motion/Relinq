using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Parsing;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.QueryParserTest
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
  }
}