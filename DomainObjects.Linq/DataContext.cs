using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Mixins;

namespace Remotion.Data.DomainObjects.Linq
{
  public static class DataContext
  {
    static DataContext ()
    {
      ResetSqlGenerator();
      // TODO: register all OPF-specific parsers in SqlGenerator.DetailParser
    }

    public static void ResetSqlGenerator ()
    {
      SqlGenerator = ObjectFactory.Create<SqlServerGenerator>().With (DatabaseInfo.Instance);
    }

    public static ISqlGeneratorBase SqlGenerator { get; set; }
    
    public static DomainObjectQueryable<T> Entity<T> ()
      where T : DomainObject
    {
      return new DomainObjectQueryable<T> (SqlGenerator);
    }
  }
}