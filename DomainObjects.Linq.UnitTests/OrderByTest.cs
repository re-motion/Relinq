using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class OrderByTest
  {

    [Test]
    public void InitializeWithOneOrdering()
    {
      Ordering ordering = CreateOrdering();
      OrderBy orderBy = new OrderBy (ordering);
    }
    

    [Test]
    public void AddMoreOrderings()
    {
      Ordering ordering1 = CreateOrdering ();
      Ordering ordering2 = CreateOrdering ();
      OrderBy orderBy = new OrderBy (ordering1);
      orderBy.Add (ordering2);

    }
    
    public Ordering CreateOrdering()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      return new Ordering (expression, Ordering.OrderDirection.Asc);
    }
  }
}