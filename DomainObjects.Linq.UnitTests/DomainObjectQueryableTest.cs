using NUnit.Framework;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Rhino.Mocks;
using Remotion.Data.DomainObjects.UnitTests.TestDomain;
using Remotion.Data.Linq;

namespace Remotion.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class DomainObjectQueryableTest
  {
    private SqlGeneratorBase _sqlGenerator;

    [SetUp]
    public void Setup ()
    {
      _sqlGenerator = new SqlServerGenerator (DatabaseInfo.Instance);
    }

    [Test]
    public void DomainObjectQueryable_Executor()
    {
      DomainObjectQueryable<Order> queryable = new DomainObjectQueryable<Order> (_sqlGenerator);
      Assert.IsNotNull (((QueryProviderBase) queryable.Provider).Executor);
    }

  }
}