using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.UnitTests.TestDomain;
using Rubicon.Data.Linq;
using System.Diagnostics;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class DomainObjectQueryableTest
  {
    [Test]
    public void DomainObjectQueryable_Executor()
    {
      DomainObjectQueryable<Order> queryable = new DomainObjectQueryable<Order> (null);
      Assert.IsNotNull (((QueryProviderBase) queryable.Provider).Executor);
    }

    [Test]
    public void DomainObjectQueryable_ExecutorAndListener ()
    {
      MockRepository repository = new MockRepository ();
      IQueryListener listener = repository.CreateMock<IQueryListener> ();

      DomainObjectQueryable<Order> queryable = new DomainObjectQueryable<Order> (listener);
      Assert.AreSame (listener, ((QueryExecutor<Order>)((QueryProviderBase) queryable.Provider).Executor).Listener);
    }
  }
}