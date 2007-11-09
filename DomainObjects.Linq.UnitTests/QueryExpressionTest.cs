using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class QueryExpressionTest
  {
    [Test]
    public void Initialize_WithFromClauseAndBody ()
    {
      FromClause fromClause = CreateFromClause();
      QueryBody queryBody = new QueryBody ();
      QueryExpression model = new QueryExpression (fromClause, queryBody);
      Assert.AreSame (fromClause, model.FromClause);
      Assert.AreSame (queryBody, model.QueryBody);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialization_ThrowsOnNullFromClause ()
    {
      new QueryExpression (null, new QueryBody());
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialization_ThrowsOnNullQueryBody ()
    {
      new QueryExpression (CreateFromClause(), null);
    }

    private FromClause CreateFromClause ()
    {
      ParameterExpression id = Expression.Parameter (typeof (int), "i");
      Expression expression = Expression.NewArrayInit (typeof (int));
      return new FromClause (id, expression);
    }
  }
}
