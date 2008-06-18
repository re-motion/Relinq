using System.Linq;
using NUnit.Framework;
using Remotion.Data.DomainObjects.UnitTests.TestDomain;
using Remotion.Data.Linq.SqlGeneration.SqlServer;

namespace Remotion.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class QueryProviderTest
  {
    [Test]
    public void CreateQuery()
    {
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (DatabaseInfo.Instance);
      QueryExecutor<Supplier> executor = new QueryExecutor<Supplier> (sqlGenerator);
      QueryProvider provider = new QueryProvider (executor);
      IQueryable<Supplier> query = from supplier in DataContext.Entity<Supplier> () select supplier;

      IQueryable<Supplier> queryCreatedByProvider = provider.CreateQuery<Supplier> (query.Expression);
      Assert.IsNotNull (queryCreatedByProvider);
      Assert.IsInstanceOfType (typeof (DomainObjectQueryable<Supplier>), queryCreatedByProvider);
    }
  }
}