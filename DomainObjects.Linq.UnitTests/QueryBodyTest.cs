using System;
using System.Linq.Expressions;
using NUnit.Framework;
using OrderDirection=Rubicon.Data.DomainObjects.Linq.OrderDirection;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class QueryBodyTest
  {
    [Test]
    public void InitializeWithISelectOrGroupClause()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      ISelectGroupClause iSelectOrGroupClause = new SelectClause (expression);

      QueryBody queryBody = new QueryBody (iSelectOrGroupClause);
    }

    [Test]
    public void InitializeWithISelectOrGroupClauseAndOrderBy()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      ISelectGroupClause iSelectOrGroupClause = new SelectClause (expression);

      OrderingClause ordering = new OrderingClause (expression, OrderDirection.Asc);

      QueryBody queryBody = new QueryBody (iSelectOrGroupClause, ordering);
    }

    [Test]
    public void AddIFromLetWhereClause()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      ISelectGroupClause iSelectOrGroupClause = new SelectClause (expression);

      QueryBody queryBody = new QueryBody (iSelectOrGroupClause);

      IFromLetWhereClause iFromLetWhereCLause = new WhereClause (expression);

      queryBody.Add (iFromLetWhereCLause);
    }
  }
}