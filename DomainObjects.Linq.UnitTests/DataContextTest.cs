using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Rubicon.Data.DomainObjects.Queries;
using Rubicon.Data.DomainObjects.UnitTests;
using Rubicon.Data.DomainObjects.UnitTests.TestDomain;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class DataContextTest : ClientTransactionBaseTest
  {
    [Test]
    public void Entity()
    {
      Assert.IsNotNull (DataContext.Entity<Order>());
    }

    [Test]
    public void Entity_WithListener()
    {
      MockRepository repository = new MockRepository();
      IQueryListener listener = repository.CreateMock<IQueryListener>();

      // expectations
      listener.QueryConstructed (null);
      LastCall.Constraints (new PredicateConstraint<Query> (delegate (Query query) { return query != null && query.Statement != null && query.Statement.Length > 0; }));

      repository.ReplayAll();

      var orders = (from order in DataContext.Entity<Order> (listener) select order).ToArray();

      repository.VerifyAll();
    }
  }
}