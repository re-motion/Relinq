using System.Linq;

namespace Rubicon.Data.Linq.UnitTests.TestQueryGenerators
{
  public class SubQueryTestQueryGenerator
  {
    public static IQueryable<Student> CreateSimpleSubQueryInAdditionalFromClause(IQueryable<Student> source)
    {
      return from s in source from s2 in (from s3 in source select s3) select s;
    }
  }
}