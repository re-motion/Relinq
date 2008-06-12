using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;

namespace Remotion.Data.DomainObjects.Linq
{
  public static class DataContext
  {
    public static DomainObjectQueryable<T> Entity<T>() 
      where T:DomainObject
    {
      return Entity<T> (null);
    }

    public static SqlGeneratorBase SqlGenerator { get; set; }
    
    public static DomainObjectQueryable<T> Entity<T> (IQueryListener listener)
      where T : DomainObject
    {
      //return new DomainObjectQueryable<T> (listener);
      SqlGenerator = new SqlServerGenerator (DatabaseInfo.Instance);
      return new DomainObjectQueryable<T> (listener, SqlGenerator);
    }
  }
}