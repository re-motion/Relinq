using System.Linq;

namespace Rubicon.Data.Linq.UnitTests.TestQueryGenerators
{
  public class SubQueryTestQueryGenerator
  {
    public static IQueryable<Student> CreateSimpleSubQueryInAdditionalFromClause(IQueryable<Student> source)
    {
      return from s in source from s2 in (from s3 in source select s3) select s;
    }

    public static IQueryable<Student> CreateComplexSubQueryInAdditionalFromClause (IQueryable<Student> source)
    {
      return from s in source from s2 in (from s3 in source where s3.ID == s.ID && s3.ID > 3 select s3) select s2;
    }

    public static IQueryable<Student> CreateSimpleSubQueryInWhereClause (IQueryable<Student> source)
    {
      return from s in source where (from s2 in source select s2).Contains (s) select s;
    }
  }
}