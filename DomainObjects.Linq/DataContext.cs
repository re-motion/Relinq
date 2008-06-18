using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;

namespace Remotion.Data.DomainObjects.Linq
{
  public static class DataContext
  {
    public static SqlGeneratorBase SqlGenerator { get; set; }
    
    public static DomainObjectQueryable<T> Entity<T> ()
      where T : DomainObject
    {
      SqlGenerator = new SqlServerGenerator (DatabaseInfo.Instance);
      return new DomainObjectQueryable<T> (SqlGenerator);
    }
  }
}