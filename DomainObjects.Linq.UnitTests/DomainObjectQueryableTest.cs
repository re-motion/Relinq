using System;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.UnitTests.TestDomain;
using Rubicon.Data.Linq;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class DomainObjectQueryableTest
  {
    [Test]
    public void DomainObjectQueryable_Executor()
    {
      DomainObjectQueryable<Order> queryable = new DomainObjectQueryable<Order>();
      Assert.IsNotNull (((QueryProviderBase) queryable.Provider).Executor);
    }
  }
}