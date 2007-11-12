using System;
using System.Linq.Expressions;
using NUnit.Framework;
using OrderDirection=Rubicon.Data.DomainObjects.Linq.OrderDirection;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class OrderByClauseTest
  {

    [Test]
    public void InitializeWithOneOrdering()
    {
      OrderingClause ordering = CreateOrdering();
      OrderByClause orderBy = new OrderByClause (ordering);
    }
    
    [Test]
    public void AddMoreOrderings()
    {
      OrderingClause ordering1 = CreateOrdering ();
      OrderingClause ordering2 = CreateOrdering ();
      OrderByClause orderBy = new OrderByClause (ordering1);
      orderBy.Add (ordering2);

    }
    
    public OrderingClause CreateOrdering()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      return new OrderingClause (expression, OrderDirection.Asc);
    }
  }
}