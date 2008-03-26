using System.Linq;
using System.Linq.Expressions;

namespace Rubicon.Data.Linq.UnitTests.TestQueryGenerators
{
  public static class DistinctTestQueryGenerator
  {
    public static IQueryable<string> CreateSimpleDistinctQuery (IQueryable<Student> source)
    {
      return (from s in source select s.First).Distinct ();
    }

    public static IQueryable<Student> CreateDisinctWithWhereQueryWithoutProjection (IQueryable<Student> source)
    {
      return (from s in source where s.First == "Garcia" select s).Distinct ();
    }

    public static IQueryable<string> CreateDisinctWithWhereQuery (IQueryable<Student> source)
    {
      return (from s in source where s.First == "Garcia" select s.First).Distinct ();
    }

    public static MethodCallExpression CreateSimpleDistinctQuery_MethodCallExpression (IQueryable<Student> source)
    {
      IQueryable<string> query = CreateSimpleDistinctQuery (source);
      MethodCallExpression newQuery = (MethodCallExpression) ((MethodCallExpression) query.Expression).Arguments[0];
      return newQuery;
    }
  }
}